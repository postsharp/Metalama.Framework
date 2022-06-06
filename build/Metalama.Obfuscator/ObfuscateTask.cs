// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Extensibility;
using PostSharp.Reflection;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeModel.Helpers;
using PostSharp.Sdk.Collections;
using PostSharp.Sdk.Extensibility;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Metalama.Obfuscator
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global

    /// <summary>
    /// A PostSharp SDK task that obfuscates the input assembly.
    /// </summary>
    public sealed class ObfuscateTask : Task
    {
        private readonly Dictionary<NamedMetadataDeclaration, string> _obfuscatedDeclarations = new( 4096 );
        private readonly Set<TypeDefDeclaration> _obfuscatedTypes = new( 1024 );

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private IType _obfuscationAttributeType;
        private ObfuscationTable _currentObfuscationTable;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        /// <summary>
        /// Initializes the task.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            this._currentObfuscationTable = new ObfuscationTable();
        }

        /// <summary>
        /// Gets or sets the path to the file where the obfuscation map will be written.
        /// </summary>
        [ConfigurableProperty]
        public string? MapFile { get; set; }

        /// <summary>
        /// Gets or sets the project path.
        /// </summary>
        [ConfigurableProperty]

        public string? RootPath { get; set; }

        /// <summary>
        /// Executes the task.
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            this._obfuscationAttributeType = (IType) this.Project.Module.Cache.GetType( typeof(ObfuscationAttribute) );

            // IMPORTANT: Pdb obfuscation has to be done before changing declaration names.
            // Obfuscate source documents in the PDB.
            this.ObfuscatePdb();

            // Invalidate the serialization of custom attributes because they may contain type references.
            // This also needs to be done before we change anything in the assembly.
            var attributeEnumerator = this.Project.Module.GetDeclarationEnumerator( TokenType.CustomAttribute );

            while ( attributeEnumerator.MoveNext() )
            {
                ((CustomAttributeDeclaration) attributeEnumerator.Current).InvalidateSerialization();
            }

            // Obfuscate types and their members.
            var typeEnumerator = this.Project.Module.GetDeclarationEnumerator( TokenType.TypeDef );

            // Scan all types to find out what must be obfuscated.
            while ( typeEnumerator.MoveNext() )
            {
                var type = (TypeDefDeclaration) typeEnumerator.Current!;

                // Ignore the type if public or serializable.
                this.PrepareType( type );
            }

            // Obfuscate.
            foreach ( var pair in this._obfuscatedDeclarations )
            {
                pair.Key.Name = pair.Value;
            }

            foreach ( var obfuscatedType in this._obfuscatedTypes )
            {
                this.UpdateMemberRefs( obfuscatedType );
            }

            // Correct MemberRefs in TypeSpecs
            foreach ( var typeSpec in this.Project.Module.TypeSpecs )
            {
                var typeDef = typeSpec.GetTypeDefinition( BindingOptions.DontThrowException );

                if ( typeDef == null || typeDef.Module != this.Project.Module )
                {
                    continue;
                }

                this.UpdateMemberRefs( typeSpec );
            }

            Message.Write(
                MessageLocation.Unknown,
                SeverityType.Info,
                "OB001",
                "{0} names hashed, {1} conflicts",
                this._currentObfuscationTable.Count,
                this._currentObfuscationTable.ConflictCount );

            if ( !string.IsNullOrEmpty( this.MapFile ) )
            {
                Message.Write( MessageLocation.Unknown, SeverityType.Info, "OB001", "Writing obfuscation map to {0}.", this.MapFile );

                using ( TextWriter writer = File.CreateText( this.MapFile! ) )
                {
                    this._currentObfuscationTable.Write( writer );
                }
            }

            return true;
        }

        private void ObfuscatePdb()
        {
            if ( this.Project.Module.SourceDocuments != null )
            {
                var rootFullPath = string.IsNullOrEmpty( this.RootPath ) ? null : Path.GetFullPath( this.RootPath! );

                foreach ( var sourceDocument in this.Project.Module.SourceDocuments )
                {
                    var mutableSourceDocument = (IMutableSourceDocument) sourceDocument;

                    if ( !string.IsNullOrEmpty( mutableSourceDocument.FileName ) )
                    {
                        string hashablePath;

                        if ( rootFullPath != null )
                        {
                            var fileFullPath = Path.GetFullPath( mutableSourceDocument.FileName );

                            hashablePath = fileFullPath.StartsWith( rootFullPath, StringComparison.Ordinal )
                                ? fileFullPath.Substring( rootFullPath.Length )
                                : fileFullPath;
                        }
                        else
                        {
                            hashablePath = mutableSourceDocument.FileName;
                        }

                        mutableSourceDocument.FileName = this._currentObfuscationTable.CreateHash( hashablePath );
                    }
                }
            }

            IMethodBodyVisitor[] visitors = { new InstructionBlockVisitor() };

            var methodEnumerator = this.Project.Module.GetDeclarationEnumerator( TokenType.MethodDef );

            while ( methodEnumerator.MoveNext() )
            {
                var method = (MethodDefDeclaration) methodEnumerator.Current!;

                if ( method.HasBody )
                {
                    method.MethodBody.CustomDebuggingInformation = null;
                    method.MethodBody.Visit( visitors, MethodBodyVisitLevel.Block );
                }
            }
        }

        private class InstructionBlockVisitor : IMethodBodyVisitor
        {
            public void EnterInstructionBlock( InstructionBlock instructionBlock )
            {
                instructionBlock.CustomDebuggingInformation = null;

                for ( var i = 0; i < instructionBlock.LocalConstantSymbolCount; i++ )
                {
                    instructionBlock.GetLocalConstantSymbol( i ).Name = "c" + i;
                }

                for ( var i = 0; i < instructionBlock.LocalVariableSymbolCount; i++ )
                {
                    instructionBlock.GetLocalVariableSymbol( i ).Name = "v" + i;
                }
            }

            public void EnterInstructionSequence( InstructionSequence instructionSequence ) { }

            public void LeaveInstructionBlock( InstructionBlock instructionBlock ) { }

            public void LeaveInstructionSequence( InstructionSequence instructionSequence ) { }

            public void VisitInstruction( InstructionReader instructionReader ) { }
        }

        private readonly TagId _excludeObfuscationTag = TagId.Register( "1F50C631-989F-48F9-B680-D64D374ED9F3" );

        private void ExcludeObfuscation( MetadataDeclaration declaration ) => declaration.SetTag<object>( this._excludeObfuscationTag, "exclude" );

        private bool IsObfuscationExcluded( MetadataDeclaration declaration ) => this.IsObfuscationExcluded( declaration, false );

        private bool IsObfuscationExcluded( MetadataDeclaration declaration, bool appliedToMembers )
        {
            bool VerifyTagsAndAttributes( MetadataDeclaration d )
            {
                if ( d.GetTag<object>( this._excludeObfuscationTag ) != null )
                {
                    return true;
                }

                // Check custom attributes.
                var e = d.CustomAttributes.GetByTypeEnumerator( this._obfuscationAttributeType );

                if ( e.MoveNext() )
                {
                    var obfuscationAttribute = (ObfuscationAttribute) e.Current!.ConstructRuntimeObject();

                    if ( obfuscationAttribute.Exclude )
                    {
                        if ( appliedToMembers )
                        {
                            return obfuscationAttribute.ApplyToMembers;
                        }

                        return true;
                    }
                }

                return false;
            }

            if ( VerifyTagsAndAttributes( declaration ) )
            {
                return true;
            }

            // Check the overridden methods and properties/events.
            switch ( declaration.GetTokenType() )
            {
                case TokenType.MethodDef:
                    {
                        var method = (MethodDefDeclaration) declaration;

                        if ( method.IsVirtual )
                        {
                            var parentDefinition = method.GetParentDefinition();

                            if ( parentDefinition != null && this.IsObfuscationExcluded( parentDefinition ) )
                            {
                                return true;
                            }
                        }

                        break;
                    }

                case TokenType.Property:
                case TokenType.Event:
                    {
                        var methodGroup = (MethodGroupDeclaration) declaration;

                        foreach ( var member in methodGroup.Members )
                        {
                            var parentDefinition = member.Method.GetParentDefinition();

                            if ( parentDefinition != null && this.IsObfuscationExcluded( parentDefinition ) )
                            {
                                return true;
                            }
                        }

                        break;
                    }
            }

            // Check the declaring type.
            switch ( declaration.GetTokenType() )
            {
                case TokenType.MethodDef:
                case TokenType.FieldDef:
                case TokenType.Property:
                case TokenType.Event:
                    return this.IsObfuscationExcluded( (MetadataDeclaration) ((IMember) declaration).DeclaringType, true );
            }

            return false;
        }

        private void PrepareType( TypeDefDeclaration type )
        {
            // Obfuscate base types first.
            if ( !this._obfuscatedTypes.AddIfAbsent( type ) )
            {
                return;
            }

            var baseType = type.BaseTypeDef;

            if ( baseType != null && baseType.Module == type.Module )
            {
                this.PrepareType( baseType );
            }

            foreach ( var interfaceImplementation in type.InterfaceImplementations )
            {
                var interfaceTypeDef = interfaceImplementation.ImplementedInterface.GetTypeDefinition();

                if ( interfaceTypeDef.Module == this.Project.Module )
                {
                    this.PrepareType( interfaceTypeDef );
                }
            }

            var isEnum = type.IsEnum();

            // Analyze implemented public interfaces.
            var implementedInterfaces = InterfaceHelper.GetAllImplementedInterfaces( type );

            // Obfuscate type names.
            if ( !type.IsPublic() &&
                 (type.Attributes & TypeAttributes.Serializable) == 0 &&
                 !this.IsObfuscationExcluded( type ) )
            {
                this._obfuscatedDeclarations.Add( type, this._currentObfuscationTable.CreateHash( type.Name, type.DeclaringType == null ) );
            }

            // Obfuscate generic parameter names.
            foreach ( var genericParameter in type.GenericParameters )
            {
                genericParameter.Name = string.Format( CultureInfo.InvariantCulture, "!{0:x}", genericParameter.Ordinal );
            }

            // Don't do anything else for delegates.
            if ( type.IsDelegate() )
            {
                return;
            }

            // Obfuscate properties.
            foreach ( var property in type.Properties.ToArray() )
            {
                if ( !property.IsPublic() )
                {
                    if ( this.IsObfuscationExcluded( property ) )
                    {
                        foreach ( var member in property.Members )
                        {
                            this.ExcludeObfuscation( member.Method );
                        }

                        continue;
                    }

                    type.Properties.Remove( property );
                }
            }

            // Obfuscate events.
            foreach ( var @event in type.Events.ToArray() )
            {
                if ( !@event.IsPublic() )
                {
                    if ( this.IsObfuscationExcluded( @event ) )
                    {
                        foreach ( var member in @event.Members )
                        {
                            this.ExcludeObfuscation( member.Method );
                        }

                        continue;
                    }

                    type.Events.Remove( @event );
                }
            }

            // Obfuscate methods.
            foreach ( var method in type.Methods )
            {
                // Ignore methods with special names.
                if ( (method.Attributes & MethodAttributes.RTSpecialName) != 0 )
                {
                    continue;
                }

                var mustNotRename = false;
                var methodsRequiringExplicitOverrideIfNotRenamed = new List<IMethod>();

                if ( method.IsVirtual )
                {
                    // Do not obfuscate virtual methods that override a public or an external method.
                    var baseMethod = method.GetParentDefinition( true );

                    if ( baseMethod.Module != this.Project.Module ||
                         (baseMethod != method && !this._obfuscatedDeclarations.ContainsKey( baseMethod )) )
                    {
                        mustNotRename = true;
                    }

                    // If the method is an implicit implementation of a public interface,
                    if ( method.Visibility == Visibility.Public )
                    {
                        foreach ( var implementedInterfaceType in implementedInterfaces )
                        {
                            var implementedInterfaceTypeDef = implementedInterfaceType.GetTypeDefinition();

                            if ( implementedInterfaceTypeDef == type )
                            {
                                continue;
                            }

                            foreach ( var interfaceMethod in implementedInterfaceTypeDef.Methods.GetByName( method.Name ) )
                            {
                                // Compare the signature.
                                if ( interfaceMethod.Parameters.Count != method.Parameters.Count )
                                {
                                    continue;
                                }

                                var translatedInterfaceMethod = (IGenericMethodDefinition) interfaceMethod.Translate( this.Project.Module );

                                var genericMap = new GenericMap(
                                    this.Project.Module,
                                    implementedInterfaceType.GetGenericContext(),
                                    translatedInterfaceMethod.GetGenericContext()
                                        .GetGenericMethodParameters() );

                                if ( !translatedInterfaceMethod.MapGenericArguments( genericMap ).DefinitionMatchesReference( method ) )
                                {
                                    continue;
                                }

                                // If this interface method is renamed, we may just rename the current method.
                                if ( this._obfuscatedDeclarations.ContainsKey( interfaceMethod ) )
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

                if ( !mustNotRename )
                {
                    // Ignore the member if it is public.
                    if ( method.IsPublic() )
                    {
                        mustNotRename = true;
                    }

                    if ( this.IsObfuscationExcluded( method ) )
                    {
                        mustNotRename = true;
                    }

                    // Dont obfuscate P-Invoke.
                    if ( (method.Attributes & MethodAttributes.PinvokeImpl) != 0 )
                    {
                        mustNotRename = true;
                    }
                }

                if ( mustNotRename )
                {
                    foreach ( var interfaceMethod in methodsRequiringExplicitOverrideIfNotRenamed )
                    {
                        method.InterfaceImplementations.Add( interfaceMethod );
                    }
                }
                else
                {
                    // Obfuscate the name of the current method.
                    this._obfuscatedDeclarations.Add( method, this._currentObfuscationTable.CreateHash( method.Name ) );
                }

                if ( !method.IsPublic() )
                {
                    // Obfuscate parameter names.
                    foreach ( var parameter in method.Parameters )
                    {
                        parameter.Name = string.Format( CultureInfo.InvariantCulture, "_{0:x}", parameter.Ordinal );
                    }

                    // Obfuscate generic parameter names.
                    foreach ( var genericParameter in method.GenericParameters )
                    {
                        genericParameter.Name = string.Format( CultureInfo.InvariantCulture, "??{0:x}", genericParameter.Ordinal );
                    }
                }
            }

            // Check that implementations of interfaces implemented by the current type are actually in the current type and not in a base type.
            // We don't support the case where the implementation is inherited.
            foreach ( var interfaceImpl in type.InterfaceImplementations )
            {
                if ( !interfaceImpl.ImplementedInterface.GetTypeDefinition().IsPublic() )
                {
                    continue;
                }

                foreach ( var interfaceMethod in interfaceImpl.ImplementedInterface.GetTypeDefinition().Methods )
                {
                    var implementations = interfaceMethod.FindGenericInterfaceOverride( type, true );

                    foreach ( var implementation in implementations )
                    {
                        if ( implementation.DeclaringType != type &&
                             implementation.Method.Visibility == Visibility.Public &&
                             this._obfuscatedDeclarations.ContainsKey( implementation.Method ) )
                        {
                            Message.Write(
                                implementation.Method,
                                SeverityType.Error,
                                "OB002",
                                "Method {0} implements the public interface method {1} in type {2} and must be manually excluded from obfuscation",
                                implementation.Method,
                                interfaceMethod,
                                type );
                        }
                    }
                }
            }

            // Obfuscate fields.
            foreach ( var field in type.Fields.ToArray() )
            {
                if ( this.IsObfuscationExcluded( field ) )
                {
                    continue;
                }

                // Ignore public fields and fields of serializable types, or fields with a special name.
                if ( field.IsPublic() ||
                     ((field.DeclaringType.Attributes & TypeAttributes.Serializable) != 0 &&
                      (field.Attributes & (FieldAttributes.NotSerialized | FieldAttributes.SpecialName | FieldAttributes.RTSpecialName)) != 0) )
                {
                    continue;
                }

                if ( field.IsConst )
                {
                    type.Fields.Remove( field );

                    continue;
                }

                if ( isEnum )
                {
                    continue;
                }

                this._obfuscatedDeclarations.Add( field, this._currentObfuscationTable.CreateHash( field.Name ) );
            }
        }

        private void UpdateMemberRefs( IMemberRefResolutionScope scope )
        {
            var refDeclarations = new List<MemberRefDeclaration>();

            // Collect FieldRefs.
            foreach ( var fieldRef in scope.FieldRefs )
            {
                if ( fieldRef.GetFieldDefinition( BindingOptions.DontThrowException ) != null )
                {
                    continue;
                }

                refDeclarations.Add( fieldRef );
            }

            // Collect MethodRefs.
            foreach ( var methodRef in scope.MethodRefs )
            {
                if ( methodRef.GetMethodDefinition( BindingOptions.DontThrowException ) != null )
                {
                    continue;
                }

                refDeclarations.Add( methodRef );
            }

            // Update them afterwards to avoid "collection modified" exceptions.
            foreach ( var memberRef in refDeclarations )
            {
                memberRef.Name = this._currentObfuscationTable.GetHash( memberRef.Name );
            }
        }
    }
}