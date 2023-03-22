// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.SyntaxSerialization;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Metalama.Framework.Engine.CompileTime.Serialization;

internal sealed class SerializerGenerator : ISerializerGenerator
{
    private const string _serializerTypeName = "Serializer";
    private readonly IDiagnosticAdder _diagnosticAdder;
    private readonly SyntaxGenerationContext _context;
    private readonly CompilationContext _runTimeCompilationContext;
    private readonly CompilationContext _compileTimeCompilationContext;
    private readonly Dictionary<AssemblyIdentity, CompileTimeProject> _referencedProjects;

    public SerializerGenerator(
        IDiagnosticAdder diagnosticAdder,
        CompilationContext runTimeCompilationContext,
        CompilationContext compileTimeCompilationContext,
        SyntaxGenerationContext context,
        IEnumerable<CompileTimeProject> compileTimeProjects )
    {
        this._diagnosticAdder = diagnosticAdder;
        this._context = context;
        this._referencedProjects = compileTimeProjects.ToDictionary( x => x.RunTimeIdentity, x => x );
        this._runTimeCompilationContext = runTimeCompilationContext;
        this._compileTimeCompilationContext = compileTimeCompilationContext;
    }

    public bool ShouldSuppressReadOnly( SerializableTypeInfo serializableType, ISymbol memberSymbol )
    {
        var serializableTypeMember = serializableType.SerializedMembers.Single( x => this._runTimeCompilationContext.SymbolComparer.Equals( x, memberSymbol ) );

        return this.ClassifyFieldOrProperty( serializableTypeMember ) == FieldOrPropertyDeserializationKind.Deserialize_MakeMutable;
    }

    public MemberDeclarationSyntax? CreateDeserializingConstructor( SerializableTypeInfo serializableType, in QualifiedTypeNameInfo serializableTypeName )
    {
        var targetType = serializableType.Type.AssertNotNull();
        var baseType = serializableType.Type.BaseType.AssertNotNull();
        var compileTimeSerializableType = this._runTimeCompilationContext.ReflectionMapper.GetTypeSymbol( typeof(ICompileTimeSerializable) );

        bool hasDeserializingBaseConstructor;

        if ( !baseType.AllInterfaces.Any( i => this._runTimeCompilationContext.SymbolComparer.Equals( i, compileTimeSerializableType ) ) )
        {
            // The base type is not compile-time serializable. It must have parameterless constructor, otherwise it's an error.
            var baseConstructor =
                baseType
                    .Constructors
                    .SingleOrDefault(
                        x =>
                            x is { Parameters: [], DeclaredAccessibility: Accessibility.Public or Accessibility.Protected } );

            if ( baseConstructor == null )
            {
                this._diagnosticAdder.Report(
                    SerializationDiagnosticDescriptors.MissingParameterlessConstructor.CreateRoslynDiagnostic(
                        targetType.GetDiagnosticLocation(),
                        (targetType, baseType) ) );

                return null;
            }

            hasDeserializingBaseConstructor = false;
        }
        else if ( !this._runTimeCompilationContext.SymbolComparer.Equals( targetType.ContainingAssembly, baseType.ContainingAssembly ) )
        {
            INamedTypeSymbol? translatedBaseType = null;

            if ( this._referencedProjects.TryGetValue( baseType.ContainingAssembly.Identity, out var referencedProject ) )
            {
                var baseTypeReflectionType = referencedProject.GetType( baseType.GetFullName().AssertNotNull() );
                translatedBaseType = (INamedTypeSymbol) this._compileTimeCompilationContext.ReflectionMapper.GetTypeSymbol( baseTypeReflectionType );
            }

            if ( translatedBaseType == null )
            {
                throw new AssertionFailedException( $"Could not translate {baseType} into the compile-time assembly." );
            }

            // The base type is outside of current assembly, check that it has the deserializing constructor.
            var baseConstructor =
                translatedBaseType
                    .Constructors
                    .SingleOrDefault(
                        x =>
                            x is { Parameters: [{ CustomModifiers: [], RefCustomModifiers: [], RefKind: RefKind.None }] }
                            && this._compileTimeCompilationContext.SymbolComparer.Equals(
                                x.Parameters[0].Type,
                                this._compileTimeCompilationContext.ReflectionMapper.GetTypeSymbol( typeof(IArgumentsReader) ) )
                            && x.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected );

            if ( baseConstructor != null )
            {
                hasDeserializingBaseConstructor = true;
            }
            else
            {
                if ( baseType.ContainingAssembly.Name == "Metalama.Framework" )
                {
                    hasDeserializingBaseConstructor = false;
                }
                else
                {
                    this._diagnosticAdder.Report(
                        SerializationDiagnosticDescriptors.MissingDeserializingConstructor.CreateRoslynDiagnostic(
                            targetType.GetDiagnosticLocation(),
                            (targetType, baseType) ) );

                    return null;
                }
            }
        }
        else
        {
            // Otherwise the base type is serializable and in the same assembly, in which case existence of the correct constructor can be presumed.
            hasDeserializingBaseConstructor = true;
        }

        const string argumentReaderParameterName = "reader";

        var body =
            this.CreateFieldDeserializationStatements(
                serializableType,
                ThisExpression(),
                IdentifierName( argumentReaderParameterName ),
                this.SelectConstructorDeserializedFields,
                this.SelectConstructorDefaultFields );

        // TODO: Browsability attributes.
        return
            ConstructorDeclaration(
                List<AttributeListSyntax>(),
                TokenList( serializableType.Type.IsValueType ? Token( SyntaxKind.PrivateKeyword ) : Token( SyntaxKind.ProtectedKeyword ) ),
                serializableTypeName.ShortName,
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

    public TypeDeclarationSyntax? CreateSerializerType( SerializableTypeInfo serializableType, in QualifiedTypeNameInfo serializableTypeName )
    {
        var members = new List<MemberDeclarationSyntax>();

        // Base serializer is always compile-time compilation symbol (if the base type is in the currently built compilation, this symbol would be the closest base
        // defined in another assembly (e.g. ReferenceTypeSerializer).
        var baseSerializerType = this.GetBaseSerializer( serializableType.Type );

        if ( baseSerializerType == null )
        {
            // Error was reported inside GetBaseSerializer.
            return null;
        }

        // Get base serializer ctor.
        var baseCtor = baseSerializerType.Constructors.SingleOrDefault( c => c.Parameters.Length == 0 );

        if ( baseCtor == null )
        {
            this._diagnosticAdder.Report(
                SerializationDiagnosticDescriptors.MissingBaseSerializerConstructor.CreateRoslynDiagnostic(
                    serializableType.Type.GetDiagnosticLocation(),
                    (serializableType.Type, baseSerializerType) ) );

            return null;
        }

        members.Add( CreateSerializerConstructor() );

        if ( serializableType.Type.IsValueType )
        {
            members.Add( this.CreateValueTypeSerializeMethod( serializableType, baseSerializerType ) );
            members.Add( this.CreateValueTypeDeserializeMethod( serializableTypeName, baseSerializerType ) );
        }
        else
        {
            members.Add( this.CreateCreateInstanceMethod( serializableType, serializableTypeName, baseSerializerType ) );
            members.Add( this.CreateReferenceTypeSerializeMethod( serializableType, serializableTypeName, baseSerializerType ) );
            members.Add( this.CreateReferenceTypeDeserializeMethod( serializableType, serializableTypeName, baseSerializerType ) );
        }

        var baseType = this.HasPendingBaseSerializer( serializableType.Type, baseSerializerType )
            ? SimpleBaseType( CreatePendingSerializerType( serializableType.Type.BaseType.AssertNotNull() ) )
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

        TypeSyntax CreatePendingSerializerType( ITypeSymbol declaringType )
        {
            return QualifiedName( (NameSyntax) this._context.SyntaxGenerator.Type( declaringType ), IdentifierName( _serializerTypeName ) );
        }
    }

    private bool HasPendingBaseSerializer( ITypeSymbol serializedType, ITypeSymbol baseSerializerSymbol )
    {
        if ( !this._runTimeCompilationContext.SymbolComparer.Equals(
                serializedType.ContainingAssembly,
                serializedType.BaseType.AssertNotNull().ContainingAssembly ) )
        {
            // The base is defined in a different assembly.
            return false;
        }

        if ( this._runTimeCompilationContext.SymbolComparer.Equals( serializedType.BaseType, baseSerializerSymbol.ContainingType ) )
        {
            // The base type serializer is in the immediate base type (there are no "pending" types in between).
            return false;
        }

        if ( !serializedType.BaseType.AssertNotNull()
                .AllInterfaces.Contains(
                    this._runTimeCompilationContext.ReflectionMapper.GetTypeSymbol( typeof(ICompileTimeSerializable) ),
                    this._runTimeCompilationContext.SymbolComparer ) )
        {
            // The base type is not serializable.
            return false;
        }

        return true;
    }

    private INamedTypeSymbol? GetBaseSerializer( INamedTypeSymbol targetType )
    {
        Invariant.Assert( targetType.BaseType != null );
        Invariant.Assert( targetType.BelongsToCompilation( this._runTimeCompilationContext ) == true );

        if ( targetType.IsValueType )
        {
            // Value type serializers are always based on ValueTypeSerializer.
            return ((INamedTypeSymbol) this._runTimeCompilationContext.ReflectionMapper.GetTypeSymbol( typeof(ValueTypeSerializer<>) )).Construct( targetType );
        }

        if ( targetType.BaseType.AllInterfaces.Contains(
                this._runTimeCompilationContext.ReflectionMapper.GetTypeSymbol( typeof(ICompileTimeSerializable) ),
                this._runTimeCompilationContext.SymbolComparer ) )
        {
            // The base type should have a serializer.
            var baseSerializer = targetType.BaseType.GetTypeMembers( _serializerTypeName ).SingleOrDefault();

            if ( baseSerializer != null && baseSerializer.IsVisibleTo( this._runTimeCompilationContext.Compilation, targetType ) )
            {
                return baseSerializer;
            }
            else
            {
                if ( this._runTimeCompilationContext.SymbolComparer.Equals( targetType.ContainingAssembly, targetType.BaseType.ContainingAssembly ) )
                {
                    // This serializer is to be generated, we will recursively look for it's base, which should have same semantics.
                    return this.GetBaseSerializer( targetType.BaseType );
                }
                else if ( targetType.BaseType.ContainingAssembly.Name == "Metalama.Framework" )
                {
                    // This is a serializable base type that does not have anything to serialize. 
                    return (INamedTypeSymbol) this._runTimeCompilationContext.ReflectionMapper.GetTypeSymbol( typeof(ReferenceTypeSerializer) );
                }
                else if ( this._referencedProjects.TryGetValue( targetType.BaseType.ContainingAssembly.Identity, out var referencedProject ) )
                {
                    // We are probably looking in the run-time assembly, but the serializer can be in the compile-time assembly.

                    var baseReflectionType = referencedProject.GetType( targetType.BaseType.GetFullName().AssertNotNull() );
                    var baseTypeSymbol = this._compileTimeCompilationContext.ReflectionMapper.GetTypeSymbol( baseReflectionType );
                    baseSerializer = baseTypeSymbol.GetTypeMembers( _serializerTypeName ).FirstOrDefault();

                    if ( baseSerializer == null )
                    {
                        this._diagnosticAdder.Report(
                            SerializationDiagnosticDescriptors.MissingBaseSerializer.CreateRoslynDiagnostic(
                                targetType.GetDiagnosticLocation(),
                                (targetType, targetType.BaseType) ) );

                        return null;
                    }

                    return baseSerializer;
                }
                else
                {
                    this._diagnosticAdder.Report(
                        SerializationDiagnosticDescriptors.MissingBaseSerializer.CreateRoslynDiagnostic(
                            targetType.GetDiagnosticLocation(),
                            (targetType, targetType.BaseType) ) );

                    return null;
                }
            }
        }
        else
        {
            // This is first serializer in the hierarchy.
            return (INamedTypeSymbol) this._compileTimeCompilationContext.ReflectionMapper.GetTypeSymbol( typeof(ReferenceTypeSerializer) );
        }
    }

    private static ConstructorDeclarationSyntax CreateSerializerConstructor()
        =>

            // TODO: We probably don't need the constructor for anything, the base should have parameterless constructor.
            ConstructorDeclaration(
                List<AttributeListSyntax>(),
                TokenList( Token( SyntaxKind.PublicKeyword ) ),
                Identifier( _serializerTypeName ),
                ParameterList(),
                null,
                SyntaxFactoryEx.FormattedBlock(),
                null );

    private MethodDeclarationSyntax CreateCreateInstanceMethod(
        SerializableTypeInfo serializedType,
        in QualifiedTypeNameInfo serializedTypeName,
        INamedTypeSymbol baseSerializer )
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
                SyntaxFactoryEx.FormattedBlock(
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
                SyntaxFactoryEx.FormattedBlock(
                    ReturnStatement(
                        Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( Space ),
                        ObjectCreationExpression(
                            serializedTypeName.QualifiedName,
                            ArgumentList( SingletonSeparatedList( Argument( IdentifierName( createInstanceMethod.Parameters[1].Name ) ) ) ),
                            null ),
                        Token( SyntaxKind.SemicolonToken ) ) );
        }

        return this.CreateOverrideMethod(
            createInstanceMethod,
            body );
    }

    private static LocalDeclarationStatementSyntax CreateTypedLocalVariable(
        in QualifiedTypeNameInfo type,
        ExpressionSyntax untypedExpression,
        out string name )
    {
        const string typedVariableName = "typedObj";

        name = typedVariableName;

        return
            LocalDeclarationStatement(
                VariableDeclaration(
                    type.QualifiedName,
                    SingletonSeparatedList(
                        VariableDeclarator(
                            Identifier( typedVariableName ),
                            null,
                            EqualsValueClause(
                                SyntaxFactoryEx.SafeCastExpression(
                                    type.QualifiedName,
                                    untypedExpression ) ) ) ) ) );
    }

    private MethodDeclarationSyntax CreateReferenceTypeSerializeMethod(
        SerializableTypeInfo serializedType,
        in QualifiedTypeNameInfo serializedTypeName,
        INamedTypeSymbol baseSerializer )
    {
        BlockSyntax body;

        var baseSerializeMethod = baseSerializer.GetMembers()
            .OfType<IMethodSymbol>()
            .Single( x => x.Name == nameof(ReferenceTypeSerializer.SerializeObject) );

        Invariant.Assert( baseSerializeMethod.Parameters.Length == 3 );

        if ( serializedType.SerializedMembers.Count > 0 )
        {
            var localVariableDeclaration =
                CreateTypedLocalVariable( serializedTypeName, IdentifierName( baseSerializeMethod.Parameters[0].Name ), out var localVariableName );

            body = Block(
                CreateBaseCallStatement(),
                localVariableDeclaration,
                this.CreateFieldSerializationStatements(
                    serializedType,
                    IdentifierName( localVariableName ),
                    IdentifierName( baseSerializeMethod.Parameters[1].Name ),
                    IdentifierName( baseSerializeMethod.Parameters[2].Name ) ) );
        }
        else
        {
            body = SyntaxFactoryEx.FormattedBlock( CreateBaseCallStatement() );
        }

        return this.CreateOverrideMethod(
            baseSerializeMethod,
            body );

        StatementSyntax CreateBaseCallStatement()
        {
            return
                baseSerializeMethod.IsAbstract && !this.HasPendingBaseSerializer( serializedType.Type, baseSerializer )
                ? EmptyStatement()
                : ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            BaseExpression(),
                            IdentifierName( nameof( ReferenceTypeSerializer.SerializeObject ) ) ),
                        ArgumentList( SeparatedList( baseSerializeMethod.Parameters.Select( p => Argument( IdentifierName( p.Name ) ) ) ) ) ) );
        }
    }

    private MethodDeclarationSyntax CreateReferenceTypeDeserializeMethod(
        SerializableTypeInfo serializedType,
        in QualifiedTypeNameInfo serializedTypeName,
        INamedTypeSymbol baseSerializer )
    {
        var baseDeserializeMethod = baseSerializer.GetMembers()
            .OfType<IMethodSymbol>()
            .Single( x => x.Name == nameof(ReferenceTypeSerializer.DeserializeFields) );

        Invariant.Assert( baseDeserializeMethod.Parameters.Length == 2 );

        BlockSyntax body;

        if ( serializedType.SerializedMembers.Count > 0 )
        {
            var localVariableDeclaration =
                CreateTypedLocalVariable( serializedTypeName, IdentifierName( baseDeserializeMethod.Parameters[0].Name ), out var localVariableName );

            body = Block(
                CreateBaseCallStatement(),
                localVariableDeclaration,
                this.CreateFieldDeserializationStatements(
                    serializedType,
                    IdentifierName( localVariableName ),
                    IdentifierName( baseDeserializeMethod.Parameters[1].Name ),
                    this.SelectLateDeserializedFields,
                    static _ => Array.Empty<ISymbol>() ) );
        }
        else
        {
            body = SyntaxFactoryEx.FormattedBlock( CreateBaseCallStatement() );
        }

        return this.CreateOverrideMethod(
            baseSerializer.GetMembers().OfType<IMethodSymbol>().Single( x => x.Name == nameof(ReferenceTypeSerializer.DeserializeFields) ),
            body );

        StatementSyntax CreateBaseCallStatement()
        {
            return
                baseDeserializeMethod.IsAbstract && !this.HasPendingBaseSerializer( serializedType.Type, baseSerializer )
                ? EmptyStatement()
                : ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            BaseExpression(),
                            IdentifierName( nameof( ReferenceTypeSerializer.DeserializeFields ) ) ),
                            ArgumentList( SeparatedList( baseDeserializeMethod.Parameters.Select( p => Argument( IdentifierName( p.Name ) ) ) ) ) ) );
        }
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

    private MethodDeclarationSyntax CreateValueTypeDeserializeMethod( in QualifiedTypeNameInfo serializedTypeName, INamedTypeSymbol baseSerializer )
    {
        var deserializeMethod = baseSerializer.GetMembers()
            .OfType<IMethodSymbol>()
            .Single( x => x.Name == nameof(ValueTypeSerializer<int>.DeserializeObject) );

        Invariant.Assert( deserializeMethod.Parameters.Length == 1 );

        var body =
            Block(
                ReturnStatement(
                    Token( SyntaxKind.ReturnKeyword ).WithTrailingTrivia( Space ),
                    ObjectCreationExpression(
                        serializedTypeName.QualifiedName,
                        ArgumentList( SingletonSeparatedList( Argument( IdentifierName( deserializeMethod.Parameters[0].Name ) ) ) ),
                        null ),
                    Token( SyntaxKind.SemicolonToken ) ) );

        return this.CreateOverrideMethod(
            baseSerializer.GetMembers().OfType<IMethodSymbol>().Single( x => x.Name == nameof(ValueTypeSerializer<int>.DeserializeObject) ),
            body );
    }

    private MethodDeclarationSyntax CreateOverrideMethod( IMethodSymbol methodSymbol, BlockSyntax body )
        => MethodDeclaration(
            List<AttributeListSyntax>(),
            TokenList( Token( SyntaxKind.PublicKeyword ).WithTrailingTrivia( Space ), Token( SyntaxKind.OverrideKeyword ).WithTrailingTrivia( Space ) ),
            this._context.SyntaxGenerator.Type( methodSymbol.ReturnType ).WithTrailingTrivia( Space ),
            null,
            Identifier( methodSymbol.Name ),
            null,
            ParameterList(
                SeparatedList(
                    methodSymbol.Parameters.Select( p => Parameter( Identifier( p.Name ) ).WithType( this._context.SyntaxGenerator.Type( p.Type ) ) ) ) ),
            List<TypeParameterConstraintClauseSyntax>(),
            body,
            null );

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
        Func<SerializableTypeInfo, IEnumerable<ISymbol>> deserializedLocationSelector,
        Func<SerializableTypeInfo, IEnumerable<ISymbol>> defaultLocationSelector )
    {
        var statements = new List<StatementSyntax>();

        foreach ( var member in deserializedLocationSelector( serializedType ) )
        {
            var memberType =
                member switch
                {
                    IFieldSymbol field => field.Type,
                    IPropertySymbol property => property.Type,
                    _ => throw new AssertionFailedException( $"Invalid member type: {member.Kind}." )
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

        foreach ( var member in defaultLocationSelector( serializedType ) )
        {
            statements.Add(
                ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            targetExpression,
                            IdentifierName( member.Name ) ),
                        LiteralExpression( SyntaxKind.DefaultLiteralExpression ) ) ) );
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

    private IEnumerable<ISymbol> SelectConstructorDefaultFields( SerializableTypeInfo serializableType )
    {
        var constructorDeserializedMembers = new HashSet<ISymbol>( this._runTimeCompilationContext.SymbolComparer );

        foreach ( var serializedMember in serializableType.SerializedMembers )
        {
            if ( this.ClassifyFieldOrProperty( serializedMember ) == FieldOrPropertyDeserializationKind.Constructor )
            {
                constructorDeserializedMembers.Add( serializedMember );
            }
        }

        foreach ( var member in serializableType.Type.GetMembers().Where( this.RequiresConstructorInitialization ) )
        {
            if ( !constructorDeserializedMembers.Contains( member ) )
            {
                yield return member;
            }
        }
    }

    private bool RequiresConstructorInitialization( ISymbol fieldOrProperty )
    {
        if ( fieldOrProperty.IsStatic )
        {
            return false;
        }

        if ( fieldOrProperty.GetAttributes()
            .Any(
                a => this._runTimeCompilationContext.SymbolComparer.Is(
                    a.AttributeClass.AssertNotNull(),
                    this._runTimeCompilationContext.ReflectionMapper.GetTypeSymbol( typeof(IAdviceAttribute) ) ) ) )
        {
            // Skip all template symbols.
            return false;
        }

        if ( fieldOrProperty is IFieldSymbol { IsImplicitlyDeclared: false } )
        {
            return true;
        }

        if ( fieldOrProperty is IPropertySymbol p && p.IsAutoProperty().GetValueOrDefault() )
        {
            return true;
        }

        return false;
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
            _ => throw new AssertionFailedException( $"Unexpected symbol kind: {symbol.Kind}." )
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

        foreach ( var member in type.GetMembers().Where( m => m is { IsStatic: false, Kind: SymbolKind.Field or SymbolKind.Property } ) )
        {
            if ( member.GetAttributes()
                .Any(
                    a => this._runTimeCompilationContext.SymbolComparer.Equals(
                        a.AttributeClass,
                        this._runTimeCompilationContext.ReflectionMapper.GetTypeSymbol( typeof(NonCompileTimeSerializedAttribute) ) ) ) )
            {
                continue;
            }

            if ( member is IFieldSymbol field )
            {
                // System.Int32 (and similar types) contains a private field of type Int32
                // skip it to avoid stack overflow
                if ( this._runTimeCompilationContext.SymbolComparer.Equals( field.Type, type ) )
                {
                    continue;
                }

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