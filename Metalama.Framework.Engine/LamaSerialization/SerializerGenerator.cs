﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.LamaSerialization
{
    internal class SerializerGenerator : ISerializerGenerator
    {
        private const string _serializerTypeName = "Serializer";

        private readonly SyntaxGenerationContext _context;
        private readonly ReflectionMapper _runtimeReflectionMapper;

        public SerializerGenerator( Compilation runtimeCompilation, SyntaxGenerationContext context )
        {
            this._context = context;

            this._runtimeReflectionMapper = new ReflectionMapper( runtimeCompilation );
        }

        public bool ShouldSuppressReadOnly( SerializableTypeInfo serializableType, ISymbol memberSymbol )
        {
            var serializableTypeMember = serializableType.SerializedMembers.Single( x => SymbolEqualityComparer.Default.Equals( x, memberSymbol ) );

            return this.ClassifyFieldOrProperty( serializableTypeMember ) == FieldOrPropertyDeserializationKind.Deserialize_MakeMutable;
        }

        public MemberDeclarationSyntax CreateDeserializingConstructor( SerializableTypeInfo serializableType )
        {
            var baseType = serializableType.Type.BaseType.AssertNotNull();

            while ( SymbolEqualityComparer.Default.Equals(
                       serializableType.Type.ContainingAssembly,
                       baseType.ContainingAssembly ) )
            {
                // The base type is in the same assembly, we have to walk back to get to the first type that is not in this assembly.
                // All ancestor types in this assembly will have their attributes generated.
                baseType = baseType.BaseType.AssertNotNull();
            }

            // Presume that base type is not null.
            var baseConstructors =
                baseType.AssertNotNull()
                    .GetMembers()
                    .OfType<IMethodSymbol>()
                    .Where(
                        x =>
                            x.Name == WellKnownMemberNames.InstanceConstructorName
                            && x.Parameters.Length == 1
                            && SymbolEqualityComparer.Default.Equals(
                                x.Parameters[0].Type,
                                this._runtimeReflectionMapper.GetTypeSymbol( typeof(IArgumentsReader) ) )
                            && x.Parameters[0].CustomModifiers.Length == 0
                            && x.Parameters[0].RefCustomModifiers.Length == 0 )
                    .ToArray();

            // TODO: This should become an error.
            // TODO: Custom modifiers or ref on the parameter should produce an error too.
            Invariant.Assert(
                baseConstructors.Length == 0
                || (baseConstructors.Length == 1
                    && (
                        baseConstructors[0].DeclaredAccessibility == Accessibility.Public
                        || baseConstructors[0].DeclaredAccessibility == Accessibility.Protected)) );

            var hasDeserializingBaseConstructor =
                baseConstructors.Length == 1
                || baseType != serializableType.Type.BaseType.AssertNotNull();

            const string argumentReaderParameterName = "reader";

            var body =
                this.CreateFieldDeserializationStatements(
                    serializableType,
                    ThisExpression(),
                    IdentifierName( argumentReaderParameterName ),
                    this.SelectConstructorDeserializedFields );

            // TODO: Browsability attributes.
            return
                ConstructorDeclaration(
                    List<AttributeListSyntax>(),
                    TokenList( serializableType.Type.IsValueType ? Token( SyntaxKind.PrivateKeyword ) : Token( SyntaxKind.ProtectedKeyword ) ),
                    Identifier( serializableType.Type.Name ),
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter( Identifier( argumentReaderParameterName ) )
                                .WithType( this._context.SyntaxGenerator.Type( this._context.ReflectionMapper.GetTypeSymbol( typeof(IArgumentsReader) ) ) ) ) ),
                    hasDeserializingBaseConstructor
                        ? ConstructorInitializer(
                            SyntaxKind.BaseConstructorInitializer,
                            ArgumentList( SingletonSeparatedList( Argument( IdentifierName( argumentReaderParameterName ) ) ) ) )
                        : null,
                    body,
                    null );
        }

        public TypeDeclarationSyntax CreateSerializerType( SerializableTypeInfo serializableType )
        {
            var members = new List<MemberDeclarationSyntax>();

            // Base serializer - if it does not exist in runtime compilation, the top-most existing base type is returned.
            var baseSerializerType = this.GetBaseSerializer( serializableType.Type );

            // Check that the base serializer constructor is visible.
            var baseCtor = baseSerializerType.Constructors.SingleOrDefault( c => c.Parameters.Length == 0 );

            if ( baseCtor?.DeclaredAccessibility != Accessibility.Public && baseCtor?.DeclaredAccessibility != Accessibility.Protected )
            {
                // TODO: Error.
                throw new AssertionFailedException();

                // SerializationMessageSource.Instance.Write( this.parent.baseSerializerConstructor.GetMethodDefinition(), SeverityType.Error, "SR0011",
                //                                           this.parent.baseSerializerType, targetType );
            }

            members.Add( CreateSerializerConstructor() );

            if ( serializableType.Type.IsValueType )
            {
                members.Add( this.CreateValueTypeSerializeMethod( serializableType, baseSerializerType ) );
                members.Add( this.CreateValueTypeDeserializeMethod( serializableType, baseSerializerType ) );
            }
            else
            {
                members.Add( this.CreateCreateInstanceMethod( serializableType, baseSerializerType ) );
                members.Add( this.CreateReferenceTypeSerializeMethod( serializableType, baseSerializerType ) );
                members.Add( this.CreateReferenceTypeDeserializeMethod( serializableType, baseSerializerType ) );
            }

            var baseType =
                HasPendingBaseSerializer( serializableType.Type, baseSerializerType )
                    ? SimpleBaseType( CreatePendingMetaSerializerType( serializableType.Type.BaseType.AssertNotNull() ) )
                    : SimpleBaseType( this._context.SyntaxGenerator.Type( baseSerializerType ) );

            // TODO: CompilerGenerated attribute.
            return ClassDeclaration(
                List<AttributeListSyntax>(),
                TokenList( Token( SyntaxKind.PublicKeyword ) ),
                Identifier( _serializerTypeName ),
                null,
                BaseList( Token( SyntaxKind.ColonToken ), SingletonSeparatedList<BaseTypeSyntax>( baseType ) ),
                List<TypeParameterConstraintClauseSyntax>(),
                List( members ) );

            TypeSyntax CreatePendingMetaSerializerType( ITypeSymbol declaringType )
                => QualifiedName( (NameSyntax) this._context.SyntaxGenerator.Type( declaringType ), IdentifierName( _serializerTypeName ) );
        }

        private static bool HasPendingBaseSerializer( ITypeSymbol serializedType, ITypeSymbol baseSerializerSymbol )
            => SymbolEqualityComparer.Default.Equals( serializedType.ContainingAssembly, serializedType.BaseType.AssertNotNull().ContainingAssembly )
               && !SymbolEqualityComparer.Default.Equals( serializedType.BaseType, baseSerializerSymbol.ContainingType );

        private INamedTypeSymbol GetBaseSerializer( ITypeSymbol targetType )
        {
            Invariant.Assert( targetType.BaseType != null );

            if ( targetType.IsValueType )
            {
                // Value type serializers are always based on ValueTypeMetaSerializer.
                return ((INamedTypeSymbol) this._context.ReflectionMapper.GetTypeSymbol( typeof(ValueTypeSerializer<>) )).Construct( targetType );
            }

            if ( targetType.BaseType.AllInterfaces.Contains(
                    this._runtimeReflectionMapper.GetTypeSymbol( typeof(ILamaSerializable) ),
                    SymbolEqualityComparer.Default ) )
            {
                // The base type should have a meta serializer.

                // TODO: This lookup should go through a repository.
                var baseMetaSerializer = targetType.BaseType.GetContainedSymbols()
                    .OfType<INamedTypeSymbol>()
                    .FirstOrDefault( x => StringComparer.Ordinal.Equals( x.Name, _serializerTypeName ) );

                if ( baseMetaSerializer != null )
                {
                    return baseMetaSerializer;
                }
                else
                {
                    if ( SymbolEqualityComparer.Default.Equals( targetType.ContainingAssembly, targetType.BaseType.ContainingAssembly ) )
                    {
                        // This serializer is to be generated, we will recursively look for it's base, which should have same semantics.
                        return this.GetBaseSerializer( targetType.BaseType );
                    }
                    else if ( targetType.BaseType.ContainingAssembly.Name == "Metalama.Framework" )
                    {
                        // This is a serializable base type that does not have anything to serialize. 
                        return (INamedTypeSymbol) this._context.ReflectionMapper.GetTypeSymbol( typeof(ReferenceTypeSerializer) );
                    }
                    else
                    {
                        // TODO: This is probably assembly that was not processed.
                        throw new AssertionFailedException();
                    }
                }
            }
            else
            {
                // This is first serializer in the hierarchy.
                return (INamedTypeSymbol) this._context.ReflectionMapper.GetTypeSymbol( typeof(ReferenceTypeSerializer) );
            }
        }

        private static ConstructorDeclarationSyntax CreateSerializerConstructor()
        {
            // TODO: We probably don't need the constructor for anything, the base should have parameterless constructor.
            return ConstructorDeclaration(
                List<AttributeListSyntax>(),
                TokenList( Token( SyntaxKind.PublicKeyword ) ),
                Identifier( _serializerTypeName ),
                ParameterList(),
                null,
                Block(),
                null );
        }

        private MethodDeclarationSyntax CreateCreateInstanceMethod( SerializableTypeInfo serializedType, INamedTypeSymbol baseSerializer )
        {
            var serializerBaseType = baseSerializer;

            var createInstanceMethod = serializerBaseType.GetMembers()
                .OfType<IMethodSymbol>()
                .Single( x => x.Name == nameof(ReferenceTypeSerializer.CreateInstance) );

            Invariant.Assert( createInstanceMethod.Parameters.Length == 2 );

            BlockSyntax body;

            if ( serializedType.Type.IsAbstract )
            {
                body =
                    Block(
                        ThrowStatement(
                            ObjectCreationExpression(
                                this._context.SyntaxGenerator.Type( this._context.ReflectionMapper.GetTypeSymbol( typeof(InvalidOperationException) ) ),
                                ArgumentList(
                                    SingletonSeparatedList(
                                        Argument(
                                            LiteralExpression(
                                                SyntaxKind.StringLiteralExpression,
                                                Literal( "Attempting to instantiate abstract class." ) ) ) ) ),
                                null ) ) );
            }
            else
            {
                body =
                    Block(
                        ReturnStatement(
                            ObjectCreationExpression(
                                this._context.SyntaxGenerator.Type( serializedType.Type ),
                                ArgumentList( SingletonSeparatedList( Argument( IdentifierName( createInstanceMethod.Parameters[1].Name ) ) ) ),
                                null ) ) );
            }

            return this.CreateOverrideMethod(
                createInstanceMethod,
                body );
        }

        private LocalDeclarationStatementSyntax CreateTypedLocalVariable( ITypeSymbol type, ExpressionSyntax untypedExpression, out string name )
        {
            const string typedVariableName = "typedObj";

            name = typedVariableName;

            return
                LocalDeclarationStatement(
                    VariableDeclaration(
                        this._context.SyntaxGenerator.Type( type ),
                        SingletonSeparatedList(
                            VariableDeclarator(
                                Identifier( typedVariableName ),
                                null,
                                EqualsValueClause(
                                    CastExpression(
                                        this._context.SyntaxGenerator.Type( type ),
                                        untypedExpression ) ) ) ) ) );
        }

        private MethodDeclarationSyntax CreateReferenceTypeSerializeMethod( SerializableTypeInfo serializedType, INamedTypeSymbol baseSerializer )
        {
            var baseSerializeMethod = baseSerializer.GetMembers()
                .OfType<IMethodSymbol>()
                .Single( x => x.Name == nameof(ReferenceTypeSerializer.SerializeObject) );

            Invariant.Assert( baseSerializeMethod.Parameters.Length == 3 );

            var localVariableDeclaration =
                this.CreateTypedLocalVariable( serializedType.Type, IdentifierName( baseSerializeMethod.Parameters[0].Name ), out var localVariableName );

            var body =
                Block(
                    baseSerializeMethod.IsAbstract && !HasPendingBaseSerializer( serializedType.Type, baseSerializer )
                        ? EmptyStatement()
                        : ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    BaseExpression(),
                                    IdentifierName( nameof(ReferenceTypeSerializer.SerializeObject) ) ),
                                ArgumentList( SeparatedList( baseSerializeMethod.Parameters.Select( p => Argument( IdentifierName( p.Name ) ) ) ) ) ) ),
                    localVariableDeclaration,
                    this.CreateFieldSerializationStatements(
                        serializedType,
                        IdentifierName( localVariableName ),
                        IdentifierName( baseSerializeMethod.Parameters[1].Name ),
                        IdentifierName( baseSerializeMethod.Parameters[2].Name ) ) );

            return this.CreateOverrideMethod(
                baseSerializeMethod,
                body );
        }

        private MethodDeclarationSyntax CreateReferenceTypeDeserializeMethod( SerializableTypeInfo serializedType, INamedTypeSymbol baseSerializer )
        {
            var baseDeserializeMethod = baseSerializer.GetMembers()
                .OfType<IMethodSymbol>()
                .Single( x => x.Name == nameof(ReferenceTypeSerializer.DeserializeFields) );

            Invariant.Assert( baseDeserializeMethod.Parameters.Length == 2 );

            var localVariableDeclaration =
                this.CreateTypedLocalVariable( serializedType.Type, IdentifierName( baseDeserializeMethod.Parameters[0].Name ), out var localVariableName );

            var body =
                Block(
                    baseDeserializeMethod.IsAbstract && !HasPendingBaseSerializer( serializedType.Type, baseSerializer )
                        ? EmptyStatement()
                        : ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    BaseExpression(),
                                    IdentifierName( nameof(ReferenceTypeSerializer.DeserializeFields) ) ),
                                ArgumentList( SeparatedList( baseDeserializeMethod.Parameters.Select( p => Argument( IdentifierName( p.Name ) ) ) ) ) ) ),
                    localVariableDeclaration,
                    this.CreateFieldDeserializationStatements(
                        serializedType,
                        IdentifierName( localVariableName ),
                        IdentifierName( baseDeserializeMethod.Parameters[1].Name ),
                        this.SelectLateDeserializedFields ) );

            return this.CreateOverrideMethod(
                baseSerializer.GetMembers().OfType<IMethodSymbol>().Single( x => x.Name == nameof(ReferenceTypeSerializer.DeserializeFields) ),
                body );
        }

        private MethodDeclarationSyntax CreateValueTypeSerializeMethod( SerializableTypeInfo serializedType, INamedTypeSymbol baseSerializer )
        {
            var serializeMethod = baseSerializer.GetMembers()
                .OfType<IMethodSymbol>()
                .Single( x => x.Name == nameof(ValueTypeSerializer<int>.SerializeObject) );

            Invariant.Assert( serializeMethod.Parameters.Length == 2 );

            var body = this.CreateFieldSerializationStatements(
                serializedType,
                IdentifierName( serializeMethod.Parameters[0].Name ),
                IdentifierName( serializeMethod.Parameters[1].Name ),
                null );

            return this.CreateOverrideMethod(
                serializeMethod,
                body );
        }

        private MethodDeclarationSyntax CreateValueTypeDeserializeMethod( SerializableTypeInfo serializedType, INamedTypeSymbol baseSerializer )
        {
            var deserializeMethod = baseSerializer.GetMembers()
                .OfType<IMethodSymbol>()
                .Single( x => x.Name == nameof(ValueTypeSerializer<int>.DeserializeObject) );

            Invariant.Assert( deserializeMethod.Parameters.Length == 1 );

            var body =
                Block(
                    ReturnStatement(
                        ObjectCreationExpression(
                            this._context.SyntaxGenerator.Type( serializedType.Type ),
                            ArgumentList( SingletonSeparatedList( Argument( IdentifierName( deserializeMethod.Parameters[0].Name ) ) ) ),
                            null ) ) );

            return this.CreateOverrideMethod(
                baseSerializer.GetMembers().OfType<IMethodSymbol>().Single( x => x.Name == nameof(ValueTypeSerializer<int>.DeserializeObject) ),
                body );
        }

        private MethodDeclarationSyntax CreateOverrideMethod( IMethodSymbol methodSymbol, BlockSyntax body )
        {
            return MethodDeclaration(
                List<AttributeListSyntax>(),
                TokenList( Token( SyntaxKind.PublicKeyword ), Token( SyntaxKind.OverrideKeyword ) ),
                this._context.SyntaxGenerator.Type( methodSymbol.ReturnType ),
                null,
                Identifier( methodSymbol.Name ),
                null,
                ParameterList(
                    SeparatedList(
                        methodSymbol.Parameters.Select( p => Parameter( Identifier( p.Name ) ).WithType( this._context.SyntaxGenerator.Type( p.Type ) ) ) ) ),
                List<TypeParameterConstraintClauseSyntax>(),
                body,
                null );
        }

        private BlockSyntax CreateFieldSerializationStatements(
            SerializableTypeInfo serializedType,
            ExpressionSyntax objectExpression,
            ExpressionSyntax constructorArgumentsWriterExpression,
            ExpressionSyntax? initializationArgumentsWriterExpression )
        {
            var statements = new List<StatementSyntax>();

            foreach ( var member in serializedType.SerializedMembers )
            {
                statements.Add(
                    ExpressionStatement(
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                this.ClassifyFieldOrProperty( member ) == FieldOrPropertyDeserializationKind.Constructor
                                    ? constructorArgumentsWriterExpression
                                    : initializationArgumentsWriterExpression.AssertNotNull(),
                                IdentifierName( nameof(IArgumentsWriter.SetValue) ) ),
                            ArgumentList(
                                SeparatedList(
                                    new[]
                                    {
                                        Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( member.Name ) ) ),
                                        Argument(
                                            MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                objectExpression,
                                                IdentifierName( member.Name ) ) ),
                                        Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( serializedType.Type.Name ) ) )
                                    } ) ) ) ) );
            }

            return Block( statements );
        }

        private BlockSyntax CreateFieldDeserializationStatements(
            SerializableTypeInfo serializedType,
            ExpressionSyntax targetExpression,
            ExpressionSyntax argumentsReaderExpression,
            Func<SerializableTypeInfo, IEnumerable<ISymbol>> locationSelector )
        {
            var statements = new List<StatementSyntax>();

            foreach ( var member in locationSelector( serializedType ) )
            {
                var memberType =
                    member switch
                    {
                        IFieldSymbol field => field.Type,
                        IPropertySymbol property => property.Type,
                        _ => throw new AssertionFailedException()
                    };

                statements.Add(
                    ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                targetExpression,
                                IdentifierName( member.Name ) ),
                            InvocationExpression(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    argumentsReaderExpression,
                                    GenericName(
                                        Identifier( nameof(IArgumentsReader.GetValue) ),
                                        TypeArgumentList( SingletonSeparatedList( this._context.SyntaxGenerator.Type( memberType ) ) ) ) ),
                                ArgumentList(
                                    SeparatedList(
                                        new[]
                                        {
                                            Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( member.Name ) ) ),
                                            Argument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( serializedType.Type.Name ) ) )
                                        } ) ) ) ) ) );
            }

            return Block( statements );
        }

        private IEnumerable<ISymbol> SelectConstructorDeserializedFields( SerializableTypeInfo serializableType )
        {
            foreach ( var serializedMember in serializableType.SerializedMembers )
            {
                if ( this.ClassifyFieldOrProperty( serializedMember ) == FieldOrPropertyDeserializationKind.Constructor )
                {
                    yield return serializedMember;
                }
            }
        }

        private IEnumerable<ISymbol> SelectLateDeserializedFields( SerializableTypeInfo serializableType )
        {
            foreach ( var serializedMember in serializableType.SerializedMembers )
            {
                if ( this.ClassifyFieldOrProperty( serializedMember ) == FieldOrPropertyDeserializationKind.Deserialize
                     || this.ClassifyFieldOrProperty( serializedMember ) == FieldOrPropertyDeserializationKind.Deserialize_MakeMutable )
                {
                    yield return serializedMember;
                }
            }
        }

        private FieldOrPropertyDeserializationKind ClassifyFieldOrProperty( ISymbol symbol )
        {
            // TODO: Cache?

            var (containingType, type, isReadOnly) = symbol switch
            {
                IFieldSymbol field => (field.ContainingType, field.Type, field.IsReadOnly),
                IPropertySymbol property => (property.ContainingType, property.Type, property.IsReadOnly || property.SetMethod?.IsInitOnly == true),
                _ => throw new AssertionFailedException()
            };

            if ( containingType.IsValueType )
            {
                // This field is declared in a value type type, so we can initialize all fields in the ctor.
                return FieldOrPropertyDeserializationKind.Constructor;
            }
            else
            {
                if ( isReadOnly )
                {
                    // The field is declared in a reference type type:
                    //   1) If the field is value type and contains any references, the field needs to be made mutable (reference targets may not yet be created).
                    //   2) Otherwise, we can deserialize using the deserializing constructor.
                    if ( this.ContainsAnySerializableReferences( type ) )
                    {
                        return FieldOrPropertyDeserializationKind.Deserialize_MakeMutable;
                    }
                    else
                    {
                        return FieldOrPropertyDeserializationKind.Constructor;
                    }
                }
                else
                {
                    return FieldOrPropertyDeserializationKind.Deserialize;
                }
            }
        }

        private bool ContainsAnySerializableReferences( ITypeSymbol type )
        {
            if ( !type.IsValueType )
            {
                // Field of managed reference type contains a reference implicitly.
                return true;
            }

            foreach ( var member in type.GetMembers().Where( m => !m.IsStatic && (m.Kind == SymbolKind.Field || m.Kind == SymbolKind.Property) ) )
            {
                if ( member.GetAttributes()
                    .Any(
                        a => SymbolEqualityComparer.Default.Equals(
                            a.AttributeClass,
                            this._runtimeReflectionMapper.GetTypeSymbol( typeof(LamaNonSerializedAttribute) ) ) ) )
                {
                    continue;
                }

                if ( member is IFieldSymbol field )
                {
                    if ( this.ContainsAnySerializableReferences( field.Type ) )
                    {
                        return true;
                    }
                }

                if ( member is IPropertySymbol property )
                {
                    if ( this.ContainsAnySerializableReferences( property.Type ) )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private enum FieldOrPropertyDeserializationKind
        {
            /// <summary>
            /// Location is deserialized in the constructor.
            /// </summary>
            Constructor,

            // ReSharper disable once InconsistentNaming

            /// <summary>
            /// Location is deserialized in the deserialize method and needs to be made mutable. This is case of fields/properties that contain reference type fields and are readonly.
            /// </summary>
            Deserialize_MakeMutable,

            /// <summary>
            /// Location is deserialized in the deserialize method.
            /// </summary>
            Deserialize
        }
    }
}