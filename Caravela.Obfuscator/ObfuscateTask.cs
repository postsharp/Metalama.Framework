using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PostSharp.Reflection;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeModel.Helpers;
using PostSharp.Sdk.Collections;
using PostSharp.Extensibility;
using PostSharp.Sdk.Extensibility;
using PostSharp.Sdk.Extensibility.Configuration;

namespace Caravela.Obfuscator
{
    public sealed class ObfuscateTask : Task
    {
        private readonly Dictionary<NamedMetadataDeclaration, string> obfuscatedDeclarations = new Dictionary<NamedMetadataDeclaration, string>(4096);

        private IType obfuscationAttributeType;
        private readonly Set<TypeDefDeclaration> obfuscatedTypes = new Set<TypeDefDeclaration>( 1024 );
        private ObfuscationTable currentObfuscationTable;

        protected override void Initialize()
        {
            base.Initialize();

            this.currentObfuscationTable = new ObfuscationTable();
        }

        [ConfigurableProperty]
        public string MapFile { get; set; }

        [ConfigurableProperty]
        public string RootPath { get; set; }

        public override bool Execute()
        {
            this.obfuscationAttributeType = (IType)this.Project.Module.Cache.GetType(typeof(ObfuscationAttribute));

            // IMPORTANT: Pdb obfuscation has to be done before changing declaration names.
            // Obfuscate source documents in the PDB.
            ObfuscatePdb();

            // Obfuscate types and their members.
            IEnumerator<MetadataDeclaration> typeEnumerator = this.Project.Module.GetDeclarationEnumerator(TokenType.TypeDef);

            // Scan all types to find out what must be obfuscated.
            while (typeEnumerator.MoveNext())
            {
                TypeDefDeclaration type = (TypeDefDeclaration)typeEnumerator.Current;

                // Ignore the type if public or serializable.
                PrepareType(type);
            }

            // Obfuscate.
            foreach (KeyValuePair<NamedMetadataDeclaration, string> pair in obfuscatedDeclarations)
            {
                pair.Key.Name = pair.Value;

            }

            foreach (TypeDefDeclaration obfuscatedType in obfuscatedTypes)
            {
                this.UpdateMemberRefs(obfuscatedType);
            }

            // Correct MemberRefs in TypeSpecs
            foreach (TypeSpecDeclaration typeSpec in this.Project.Module.TypeSpecs)
            {
                TypeDefDeclaration typeDef = typeSpec.GetTypeDefinition(BindingOptions.DontThrowException);
                if (typeDef == null || typeDef.Module != this.Project.Module)
                    continue;

                this.UpdateMemberRefs(typeSpec);
            }

            Message.Write(MessageLocation.Unknown, SeverityType.Info, "OB001", "{0} names hashed, {1} conflicts", this.currentObfuscationTable.Count,
                           this.currentObfuscationTable.ConflictCount);

            if (!string.IsNullOrEmpty(this.MapFile))
            {
                Message.Write(MessageLocation.Unknown, SeverityType.Info, "OB001", "Writing obfuscation map to {0}.", this.MapFile);
                using (TextWriter writer = File.CreateText(this.MapFile))
                {
                    this.currentObfuscationTable.Write(writer);
                }
            }

            return true;
        }

        private void ObfuscatePdb()
        {
            if (this.Project.Module.SourceDocuments != null)
            {
                string rootFullPath = String.IsNullOrEmpty(this.RootPath) ? null : Path.GetFullPath(this.RootPath);

                foreach (ISourceDocument sourceDocument in this.Project.Module.SourceDocuments)
                {
                    IMutableSourceDocument mutableSourceDocument = (IMutableSourceDocument) sourceDocument;
                    if (!string.IsNullOrEmpty(mutableSourceDocument.FileName))
                    {
                        string hashablePath;
                        if (rootFullPath != null)
                        {
                            string fileFullPath = Path.GetFullPath(mutableSourceDocument.FileName);

                            if (fileFullPath.StartsWith(rootFullPath))
                                hashablePath = fileFullPath.Substring(rootFullPath.Length);
                            else
                                hashablePath = fileFullPath;
                        }
                        else
                            hashablePath = mutableSourceDocument.FileName;

                        mutableSourceDocument.FileName = this.currentObfuscationTable.CreateHash(hashablePath);
                    }
                }
            }

            IMethodBodyVisitor[] visitors = new[] { new InstructionBlockVisitor() };
           
            IEnumerator<MetadataDeclaration> methodEnumerator = this.Project.Module.GetDeclarationEnumerator(TokenType.MethodDef);
            while (methodEnumerator.MoveNext())
            {
                MethodDefDeclaration method = (MethodDefDeclaration)methodEnumerator.Current;
                if (method.HasBody)
                {
                    method.MethodBody.CustomDebuggingInformation = null;
                    method.MethodBody.Visit(visitors, MethodBodyVisitLevel.Block );
                }
            }
        }

        class InstructionBlockVisitor : IMethodBodyVisitor
        {
            public void EnterInstructionBlock(InstructionBlock instructionBlock)
            {
                instructionBlock.CustomDebuggingInformation = null;
                for ( int i = 0; i < instructionBlock.LocalConstantSymbolCount; i++ )
                {
                    instructionBlock.GetLocalConstantSymbol(i).Name = "c" + i;
                }
                for (int i = 0; i < instructionBlock.LocalVariableSymbolCount; i++)
                {
                    instructionBlock.GetLocalVariableSymbol(i).Name = "v" + i;
                }
            }

            public void EnterInstructionSequence(InstructionSequence instructionSequence)
            {
                
            }

            public void LeaveInstructionBlock(InstructionBlock instructionBlock)
            {
                
            }

            public void LeaveInstructionSequence(InstructionSequence instructionSequence)
            {
                
            }

            public void VisitInstruction(InstructionReader instructionReader)
            {
                
            }
        }

        private readonly TagId excludeObfuscationTag = TagId.Register( "1F50C631-989F-48F9-B680-D64D374ED9F3" );
        private void ExcludeObfuscation(MetadataDeclaration declaration)
        {
            declaration.SetTag<object>( excludeObfuscationTag, "exclude" );
        }

        private bool IsObfuscationExcluded(MetadataDeclaration declaration)
        {
            return IsObfuscationExcluded( declaration, false );
        }

        private bool IsObfuscationExcluded(MetadataDeclaration declaration, bool inherited)
        {
            if (declaration.GetTag<object>(excludeObfuscationTag) != null)
                return true;

            IEnumerator<CustomAttributeDeclaration> e = declaration.CustomAttributes.GetByTypeEnumerator( this.obfuscationAttributeType );
            if ( e.MoveNext() )
            {
                ObfuscationAttribute obfuscationAttribute = (ObfuscationAttribute) e.Current.ConstructRuntimeObject();
                if (obfuscationAttribute.Exclude)
                {
                    if (inherited)
                        return obfuscationAttribute.ApplyToMembers;
                    else 
                        return true;
                }
            }

            switch ( declaration.GetTokenType())
            {
                case TokenType.MethodDef:
                case TokenType.FieldDef:
                case TokenType.Property:
                case TokenType.Event:
                    return IsObfuscationExcluded( (MetadataDeclaration) ((IMember) declaration).DeclaringType, true );
            }

            return false;
        }

        private void PrepareType( TypeDefDeclaration type )
        {
            // Obfuscate base types first.
            if ( !this.obfuscatedTypes.AddIfAbsent( type ) )
                return;


            TypeDefDeclaration baseType = type.BaseTypeDef;
            if ( baseType != null && baseType.Module == type.Module )
                this.PrepareType( baseType );

            foreach ( InterfaceImplementationDeclaration interfaceImplementation in type.InterfaceImplementations )
            {
                TypeDefDeclaration interfaceTypeDef = interfaceImplementation.ImplementedInterface.GetTypeDefinition();
                if ( interfaceTypeDef.Module == this.Project.Module )
                {
                    this.PrepareType(interfaceTypeDef);
                }
            }

            bool isEnum = type.IsEnum();
            

            // Analyze implemented public interfaces.
            Set<ITypeSignature> implementedInterfaces = InterfaceHelper.GetAllImplementedInterfaces( type );

            // Obfuscate type names.
            if ( !type.IsPublic() &&
                 (type.Attributes & TypeAttributes.Serializable) == 0 &&
                 !IsObfuscationExcluded( type ))
            {
                obfuscatedDeclarations.Add( type, this.currentObfuscationTable.CreateHash( type.Name ) );
            }

            // Obfuscate generic parameter names.
            foreach ( GenericParameterDeclaration genericParameter in type.GenericParameters )
            {
                genericParameter.Name = string.Format( "!{0:x}", genericParameter.Ordinal );
            }

            // Don't do anything else for delegates.
            if (type.IsDelegate())
                return;

            // Obfuscate properties.
            foreach (PropertyDeclaration property in type.Properties.ToArray())
            {
                if (!property.IsPublic())
                {
                    if (IsObfuscationExcluded(property))
                    {
                        foreach (MethodSemanticDeclaration member in property.Members)
                        {
                            ExcludeObfuscation(member.Method);
                        }
                        continue;
                    }


                    type.Properties.Remove(property);
                }
            }

            // Obfuscate events.
            foreach (EventDeclaration @event in type.Events.ToArray())
            {
                if (!@event.IsPublic())
                {
                    if (IsObfuscationExcluded(@event))
                    {
                        foreach (MethodSemanticDeclaration member in @event.Members)
                        {
                            ExcludeObfuscation(member.Method);
                        }
                        continue;
                    }

                    type.Events.Remove(@event);
                }
            }

            // Obfuscate methods.
            foreach ( MethodDefDeclaration method in type.Methods )
            {

             
                // Ignore methods with special names.
                if ( (method.Attributes & MethodAttributes.RTSpecialName) != 0 )
                    continue;

                bool mustNotRename = false;
                List<IMethod> methodsRequiringExplicitOverrideIfNotRenamed = new List<IMethod>();

                if (method.IsVirtual)
                {
                    // Do not obfuscate virtual methods that override a public or an external method.
                    MethodDefDeclaration baseMethod = method.GetParentDefinition( true );
                    if ( baseMethod.Module != this.Project.Module ||
                         (baseMethod != method && !obfuscatedDeclarations.ContainsKey( baseMethod )) )
                    {
                        mustNotRename = true;
                    }


                    // If the method is an implicit implementation of a public interface, 
                    if ( method.Visibility == Visibility.Public )
                    {
                     
                        foreach (ITypeSignature implementedInterfaceType in implementedInterfaces)
                        {
                            TypeDefDeclaration implementedInterfaceTypeDef = implementedInterfaceType.GetTypeDefinition();

                            if ( implementedInterfaceTypeDef == type ) continue;

                            foreach ( MethodDefDeclaration interfaceMethod in implementedInterfaceTypeDef.Methods.GetByName( method.Name ) )
                            {


                                // Compare the signature.
                                if ( interfaceMethod.Parameters.Count != method.Parameters.Count )
                                    continue;

                                IGenericMethodDefinition translatedInterfaceMethod = (IGenericMethodDefinition) interfaceMethod.Translate( this.Project.Module );
                                GenericMap genericMap = new GenericMap( this.Project.Module, implementedInterfaceType.GetGenericContext(),
                                                                        translatedInterfaceMethod.GetGenericContext().
                                                                            GetGenericMethodParameters() );

                                if ( !translatedInterfaceMethod.MapGenericArguments( genericMap ).DefinitionMatchesReference( method ) )
                                    continue;

                                // If this interface method is renamed, we may just rename the current method.
                                if ( this.obfuscatedDeclarations.ContainsKey( interfaceMethod ) )
                                {
                                    methodsRequiringExplicitOverrideIfNotRenamed.Add( translatedInterfaceMethod.GetGenericInstance( genericMap ) );
                                }
                                else
                                {
                                    mustNotRename = true;
                                }
                            }

                        }

                    

                    }
                }

                if (!mustNotRename)
                {
                    // Ignore the member if it is public.
                    if ( method.IsPublic() )
                        mustNotRename = true;

                    if (IsObfuscationExcluded(method))
                        mustNotRename = true;

                    // Dont obfuscate P-Invoke.
                    if ((method.Attributes & MethodAttributes.PinvokeImpl) != 0)
                        mustNotRename = true;
                }

                if (mustNotRename)
                {
                    foreach (IMethod interfaceMethod in methodsRequiringExplicitOverrideIfNotRenamed)
                    {
                        method.InterfaceImplementations.Add(interfaceMethod);
                    }
                }
                else
                {
                    // Obfuscate the name of the current method.
                    obfuscatedDeclarations.Add(method, this.currentObfuscationTable.CreateHash(method.Name));

                }


                if (!method.IsPublic())
                {
                    // Obfuscate parameter names.
                    foreach (ParameterDeclaration parameter in method.Parameters)
                    {
                        parameter.Name = string.Format("_{0:x}", parameter.Ordinal);
                    }

                    // Obfuscate generic parameter names.
                    foreach (GenericParameterDeclaration genericParameter in method.GenericParameters)
                    {
                        genericParameter.Name = string.Format("??{0:x}", genericParameter.Ordinal);
                    }
                }

          
            }

        
            // Obfuscate fields.
            foreach ( FieldDefDeclaration field in type.Fields.ToArray() )
            {
                if ( IsObfuscationExcluded(field) )
                    continue;

                // Ignore public fields and fields of serializable types, or fields with a special name.
                if ( field.IsPublic() ||
                     ((((field.DeclaringType.Attributes & TypeAttributes.Serializable) != 0)) &&
                      (field.Attributes & (FieldAttributes.NotSerialized | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName)) != 0) )
                    continue;

                if ( field.IsConst )
                {
                    type.Fields.Remove( field );
                    continue;
                }

                if ( isEnum )
                    continue;

                obfuscatedDeclarations.Add( field, this.currentObfuscationTable.CreateHash( field.Name ) );
            }

            
        }

     

        private void UpdateMemberRefs( IMemberRefResolutionScope scope )
        {
            List<MemberRefDeclaration> refDeclarations = new List<MemberRefDeclaration>();
            
            // Collect FieldRefs.
            foreach ( FieldRefDeclaration fieldRef in scope.FieldRefs )
            {
                if ( fieldRef.GetFieldDefinition( BindingOptions.DontThrowException ) != null )
                    continue;

                refDeclarations.Add( fieldRef );
            }

            // Collect MethodRefs.
            foreach ( MethodRefDeclaration methodRef in scope.MethodRefs )
            {
                if ( methodRef.GetMethodDefinition( BindingOptions.DontThrowException ) != null )
                    continue;

                refDeclarations.Add( methodRef );
            }
            
            // Update them afterwards to avoid "collection modified" exceptions.
            foreach (MemberRefDeclaration memberRef in refDeclarations)
            {
                memberRef.Name = this.currentObfuscationTable.GetHash( memberRef.Name );
            }
        }
    }
}