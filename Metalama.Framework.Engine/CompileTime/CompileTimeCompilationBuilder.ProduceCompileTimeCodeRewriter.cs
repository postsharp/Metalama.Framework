// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.LamaSerialization;
using Metalama.Framework.Engine.ReflectionMocks;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Accessibility = Microsoft.CodeAnalysis.Accessibility;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using TypeKind = Microsoft.CodeAnalysis.TypeKind;

namespace Metalama.Framework.Engine.CompileTime
{
    internal partial class CompileTimeCompilationBuilder
    {
#pragma warning disable CA1001 // Class must be disposable.

        /// <summary>
        /// Rewrites a run-time syntax tree into a compile-time syntax tree. Calls <see cref="TemplateCompiler"/> on templates,
        /// and removes run-time-only sub trees.
        /// </summary>
        private sealed class ProduceCompileTimeCodeRewriter : CompileTimeBaseRewriter
        {
            private static readonly SyntaxAnnotation _hasCompileTimeCodeAnnotation = new( "Metalama_HasCompileTimeCode" );
            private readonly Compilation _compileTimeCompilation;
            private readonly IReadOnlyDictionary<INamedTypeSymbol, SerializableTypeInfo> _serializableTypes;
            private readonly IReadOnlyDictionary<ISymbol, SerializableTypeInfo> _serializableFieldsAndProperties;
            private readonly IDiagnosticAdder _diagnosticAdder;
            private readonly TemplateCompiler _templateCompiler;
            private readonly CancellationToken _cancellationToken;
            private readonly NameSyntax _compileTimeTypeName;
            private readonly SyntaxGenerationContext _syntaxGenerationContext;
            private readonly NameSyntax _originalNameTypeSyntax;
            private readonly NameSyntax _originalPathTypeSyntax;
            private readonly ITypeSymbol _fabricType;
            private readonly ITypeSymbol _typeFabricType;
            private readonly ISerializerGenerator _serializerGenerator;
            private Context _currentContext;
            private HashSet<string>? _currentTypeTemplateNames;
            private string? _currentTypeName;

            public bool Success { get; private set; } = true;

            public bool FoundCompileTimeCode { get; private set; }

            public ProduceCompileTimeCodeRewriter(
                Compilation runTimeCompilation,
                Compilation compileTimeCompilation,
                IReadOnlyList<SerializableTypeInfo> serializableTypes,
                IDiagnosticAdder diagnosticAdder,
                TemplateCompiler templateCompiler,
                IServiceProvider serviceProvider,
                CancellationToken cancellationToken )
                : base( runTimeCompilation, serviceProvider )
            {
                this._compileTimeCompilation = compileTimeCompilation;
                this._diagnosticAdder = diagnosticAdder;
                this._templateCompiler = templateCompiler;
                this._cancellationToken = cancellationToken;
                this._currentContext = new Context( TemplatingScope.Both, this );

                this._serializableTypes =
                    serializableTypes.ToDictionary<SerializableTypeInfo, INamedTypeSymbol, SerializableTypeInfo>(
                        x => x.Type,
                        x => x,
                        SymbolEqualityComparer.Default );

                this._serializableFieldsAndProperties =
                    serializableTypes.SelectMany( x => x.SerializedMembers.Select( y => (Member: y, Type: x) ) )
                        .ToDictionary( x => x.Member, x => x.Type, SymbolEqualityComparer.Default );

                this._syntaxGenerationContext = SyntaxGenerationContext.CreateDefault( serviceProvider, compileTimeCompilation );

                // TODO: This should be probably injected as a service, but we are creating the generation context here.
                this._serializerGenerator = new SerializerGenerator( runTimeCompilation, this._syntaxGenerationContext );

                this._compileTimeTypeName = (NameSyntax)
                    this._syntaxGenerationContext.SyntaxGenerator.Type(
                        this._syntaxGenerationContext.ReflectionMapper.GetTypeSymbol( typeof(CompileTimeType) ) );

                this._originalNameTypeSyntax = (NameSyntax)
                    this._syntaxGenerationContext.SyntaxGenerator.Type(
                        this._syntaxGenerationContext.ReflectionMapper.GetTypeSymbol( typeof(OriginalIdAttribute) ) );

                this._originalPathTypeSyntax = (NameSyntax)
                    this._syntaxGenerationContext.SyntaxGenerator.Type(
                        this._syntaxGenerationContext.ReflectionMapper.GetTypeSymbol( typeof(OriginalPathAttribute) ) );

                var reflectionMapper = serviceProvider.GetRequiredService<ReflectionMapperFactory>().GetInstance( runTimeCompilation );
                this._fabricType = reflectionMapper.GetTypeSymbol( typeof(Fabric) );
                this._typeFabricType = reflectionMapper.GetTypeSymbol( typeof(TypeFabric) );
            }

            // TODO: assembly and module-level attributes?

            public override SyntaxNode? VisitAttributeList( AttributeListSyntax node ) => node.Parent is CompilationUnitSyntax ? null : node;

            public override SyntaxNode? VisitClassDeclaration( ClassDeclarationSyntax node ) => this.VisitTypeDeclaration( node ).SingleOrDefault();

            public override SyntaxNode? VisitStructDeclaration( StructDeclarationSyntax node ) => this.VisitTypeDeclaration( node ).SingleOrDefault();

            public override SyntaxNode? VisitInterfaceDeclaration( InterfaceDeclarationSyntax node ) => this.VisitTypeDeclaration( node ).SingleOrDefault();

            public override SyntaxNode? VisitRecordDeclaration( RecordDeclarationSyntax node ) => this.VisitTypeDeclaration( node ).SingleOrDefault();

            public override SyntaxNode? VisitEnumDeclaration( EnumDeclarationSyntax node )
            {
                this._cancellationToken.ThrowIfCancellationRequested();

                var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node ).AssertNotNull();
                var scope = this.SymbolClassifier.GetTemplatingScope( symbol );

                if ( scope == TemplatingScope.RunTimeOnly )
                {
                    return null;
                }
                else
                {
                    return base.VisitEnumDeclaration( node )!.WithAdditionalAnnotations( _hasCompileTimeCodeAnnotation );
                }
            }

            public override SyntaxNode? VisitDelegateDeclaration( DelegateDeclarationSyntax node )
            {
                this._cancellationToken.ThrowIfCancellationRequested();

                var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node ).AssertNotNull();
                var scope = this.SymbolClassifier.GetTemplatingScope( symbol );

                if ( scope == TemplatingScope.RunTimeOnly )
                {
                    return null;
                }
                else
                {
                    return base.VisitDelegateDeclaration( node )!.WithAdditionalAnnotations( _hasCompileTimeCodeAnnotation );
                }
            }

            private void PopulateNestedCompileTimeTypes( TypeDeclarationSyntax node, List<MemberDeclarationSyntax> list, string namePrefix )
            {
                // Compute the new name of the relocated children.
                namePrefix += node.Identifier.Text;

                if ( node.TypeParameterList is { Parameters: { Count: > 0 } } )
                {
                    // This does not guarantee the absence of conflict.
                    namePrefix += "X" + node.TypeParameterList.Parameters.Count;
                }

                namePrefix += "_";

                foreach ( var child in node.Members )
                {
                    var childSymbol = this.RunTimeCompilation.GetSemanticModel( child.SyntaxTree ).GetDeclaredSymbol( child )
                        as ITypeSymbol;

                    switch ( child )
                    {
                        case ClassDeclarationSyntax childType:
                            {
                                Invariant.Assert( childSymbol != null );

                                var childScope = this.SymbolClassifier.GetTemplatingScope( childSymbol );

                                switch ( childScope )
                                {
                                    case TemplatingScope.CompileTimeOnly:
                                        {
                                            // We have a build-time type nested in a run-time type. We have to un-nest it.

                                            // Check that the visibility is private.
                                            if ( childSymbol.DeclaredAccessibility != Accessibility.Private )
                                            {
                                                this._diagnosticAdder.Report(
                                                    TemplatingDiagnosticDescriptors.NestedCompileTypesMustBePrivate.CreateDiagnostic(
                                                        childType.Identifier.GetLocation(),
                                                        childSymbol ) );
                                            }

                                            // Check that it implements ITypeFabric.
                                            if ( !this.RunTimeCompilation.HasImplicitConversion( childSymbol, this._typeFabricType ) )
                                            {
                                                this._diagnosticAdder.Report(
                                                    TemplatingDiagnosticDescriptors.RunTimeTypesCannotHaveCompileTimeTypesExceptClasses.CreateDiagnostic(
                                                        childSymbol.GetDiagnosticLocation(),
                                                        (childSymbol, typeof(TypeFabric)) ) );

                                                this.Success = false;
                                            }

                                            // Create the [OriginalId] attribute.
                                            var originalId = DocumentationCommentId.CreateDeclarationId( childSymbol );

                                            var originalNameAttribute = Attribute( this._originalNameTypeSyntax )
                                                .WithArgumentList(
                                                    AttributeArgumentList(
                                                        SingletonSeparatedList( AttributeArgument( SyntaxFactoryEx.LiteralExpression( originalId ) ) ) ) );

                                            // Transform the type.
                                            var transformedChild = (TypeDeclarationSyntax) this.Visit( childType )!;

                                            // Rename the type and add [OriginalId].
                                            var newName = namePrefix + "" + childType.Identifier.Text;

                                            transformedChild = transformedChild
                                                .WithIdentifier( Identifier( newName ) )
                                                .WithModifiers( TokenList( Token( SyntaxKind.InternalKeyword ) ) )
                                                .WithAttributeLists(
                                                    transformedChild.AttributeLists.Add( AttributeList( SingletonSeparatedList( originalNameAttribute ) ) ) );

                                            list.Add( transformedChild );

                                            break;
                                        }

                                    case TemplatingScope.RunTimeOnly:
                                        // We have a run-time child type, and it must be further checked for un-nesting.

                                        this.PopulateNestedCompileTimeTypes( childType, list, namePrefix );

                                        break;

                                    default:
                                        this._diagnosticAdder.Report(
                                            TemplatingDiagnosticDescriptors.NeutralTypesForbiddenInNestedRunTimeTypes.CreateDiagnostic(
                                                childType.Identifier.GetLocation(),
                                                childSymbol ) );

                                        break;
                                }

                                break;
                            }

                        case BaseTypeDeclarationSyntax or DelegateDeclarationSyntax:
                            Invariant.Assert( childSymbol != null );

                            if ( this.SymbolClassifier.GetTemplatingScope( childSymbol ) == TemplatingScope.CompileTimeOnly )
                            {
                                this._diagnosticAdder.Report(
                                    TemplatingDiagnosticDescriptors.RunTimeTypesCannotHaveCompileTimeTypesExceptClasses.CreateDiagnostic(
                                        childSymbol.GetDiagnosticLocation(),
                                        (childSymbol, typeof(TypeFabric)) ) );

                                this.Success = false;
                            }

                            break;

                        // ReSharper disable once RedundantEmptySwitchSection
                        default:
                            // Non-type members of a run-time type are always run-time too and should not be copied to the compile-time assembly.
                            break;
                    }
                }
            }

            private IEnumerable<MemberDeclarationSyntax> VisitTypeDeclaration( TypeDeclarationSyntax node )
            {
                // Eliminate System.Runtime.CompilerServices.IsExternalInit.
                if ( node.Identifier.Text == "IsExternalInit" )
                {
                    var semanticModel = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree );

                    if ( semanticModel.GetDeclaredSymbol( node ) is { } type
                         && type.ContainingNamespace.ToDisplayString() == "System.Runtime.CompilerServices" )
                    {
                        // We are inserting this type anyway, so skip it if we find it in user code.
                        return Array.Empty<MemberDeclarationSyntax>();
                    }
                }

                this._cancellationToken.ThrowIfCancellationRequested();

                var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node ).AssertNotNull();

                var scope = this.SymbolClassifier.GetTemplatingScope( symbol );

                if ( scope == TemplatingScope.RunTimeOnly )
                {
                    // If the type contains compile-time nested types, we have to un-nest them.
                    var compileTimeMembers = new List<MemberDeclarationSyntax>();
                    this.PopulateNestedCompileTimeTypes( node, compileTimeMembers, "" );

                    return compileTimeMembers;
                }
                else
                {
                    var transformedNode = this.TransformCompileTimeType( node, symbol, scope );

                    return new[] { transformedNode };
                }
            }

            private TypeDeclarationSyntax TransformCompileTimeType( TypeDeclarationSyntax node, INamedTypeSymbol symbol, TemplatingScope scope )
            {
                this.FoundCompileTimeCode = true;

                this._currentTypeTemplateNames = new HashSet<string>( StringComparer.OrdinalIgnoreCase );
                this._currentTypeName = symbol.Name;

                // Add type members.

                var members = new List<MemberDeclarationSyntax>();

                using ( this.WithScope( scope ) )
                {
                    foreach ( var member in node.Members )
                    {
                        switch ( member )
                        {
                            case MethodDeclarationSyntax method:
                                members.AddRange( this.VisitMethodDeclaration( method ).AssertNoneNull() );

                                break;

                            case IndexerDeclarationSyntax:
                                throw new NotImplementedException( "Indexers are not implemented." );

                            // members.AddRange( this.VisitBasePropertyDeclaration( indexer ).AssertNoneNull() );

                            case PropertyDeclarationSyntax property:
                                members.AddRange( this.VisitBasePropertyDeclaration( property ).AssertNoneNull() );

                                break;

                            case EventDeclarationSyntax @event:
                                members.AddRange( this.VisitEventDeclaration( @event ).AssertNoneNull() );

                                break;

                            case FieldDeclarationSyntax field:
                                members.AddRange( this.VisitFieldDeclaration( field ).AssertNoneNull() );

                                break;

                            default:
                                members.Add( (MemberDeclarationSyntax) this.Visit( member ).AssertNotNull() );

                                break;
                        }
                    }
                }

                // Add non-implemented members of IAspect, IEligible and IProjectData.
                var syntaxGenerator = this._syntaxGenerationContext.SyntaxGenerator;
                var allImplementedInterfaces = symbol.SelectManyRecursive( i => i.Interfaces, throwOnDuplicate: false );

                foreach ( var implementedInterface in allImplementedInterfaces )
                {
                    if ( implementedInterface.Name is nameof(IAspect) or nameof(IEligible<IDeclaration>) or nameof(ProjectExtension) )
                    {
                        foreach ( var member in implementedInterface.GetMembers() )
                        {
                            if ( member is not IMethodSymbol method )
                            {
                                // IAspect and IEligible have only methods.
                                throw new AssertionFailedException();
                            }

                            var memberImplementation = (IMethodSymbol?) symbol.FindImplementationForInterfaceMember( member );

                            if ( memberImplementation == null || memberImplementation.ContainingType.TypeKind == TypeKind.Interface )
                            {
                                var newMethod = MethodDeclaration(
                                        default,
                                        default,
                                        syntaxGenerator.Type( method.ReturnType ),
                                        ExplicitInterfaceSpecifier( (NameSyntax) syntaxGenerator.Type( implementedInterface ) ),
                                        Identifier( method.Name ),
                                        default,
                                        ParameterList(
                                            SeparatedList(
                                                method.Parameters.Select(
                                                    p => Parameter(
                                                        default,
                                                        default,
                                                        syntaxGenerator.Type( p.Type ),
                                                        Identifier( p.Name ),
                                                        default ) ) ) ),
                                        default,
                                        Block(),
                                        default,
                                        Token( SyntaxKind.SemicolonToken ) )
                                    .NormalizeWhitespace();

                                members.Add( newMethod );
                            }
                        }
                    }
                }

                // Add serialization logic if the type is serializable and this is the primary declaration.
                if ( this._serializableTypes.TryGetValue( symbol, out var serializableType ) )
                {
                    if ( !serializableType.Type.GetMembers()
                            .Any(
                                m => m is IMethodSymbol method && method.MethodKind == MethodKind.Constructor && method.GetPrimarySyntaxReference() != null ) )
                    {
                        // There is no defined constructor, so we need to explicitly add parameterless constructor.
                        members.Add(
                            ConstructorDeclaration(
                                    List<AttributeListSyntax>(),
                                    TokenList( Token( SyntaxKind.PublicKeyword ) ),
                                    Identifier( serializableType.Type.Name ),
                                    ParameterList(),
                                    null,
                                    Block(),
                                    null )
                                .NormalizeWhitespace() );
                    }

                    members.Add( this._serializerGenerator.CreateDeserializingConstructor( serializableType ).NormalizeWhitespace() );
                    members.Add( this._serializerGenerator.CreateSerializerType( serializableType ).NormalizeWhitespace() );
                }

                var transformedNode = node.WithMembers( List( members ) ).WithAdditionalAnnotations( _hasCompileTimeCodeAnnotation );

                // If the type is a fabric, add the OriginalPath attribute.
                if ( this.RunTimeCompilation.HasImplicitConversion( symbol, this._fabricType ) )
                {
                    var originalPathAttribute = Attribute( this._originalPathTypeSyntax )
                        .WithArgumentList(
                            AttributeArgumentList(
                                SingletonSeparatedList( AttributeArgument( SyntaxFactoryEx.LiteralExpression( node.SyntaxTree.FilePath ) ) ) ) );

                    transformedNode = transformedNode
                        .WithAttributeLists( transformedNode.AttributeLists.Add( AttributeList( SingletonSeparatedList( originalPathAttribute ) ) ) );
                }

                return transformedNode;
            }

            private bool CheckTemplateName( ISymbol symbol )
            {
                if ( this._currentTypeTemplateNames!.Add( symbol.Name ) )
                {
                    // It's the first time we're seeing this name.
                    return true;
                }
                else
                {
                    this.Success = false;

                    this._diagnosticAdder.Report(
                        GeneralDiagnosticDescriptors.TemplateWithSameNameAlreadyDefined.CreateDiagnostic(
                            symbol.GetDiagnosticLocation(),
                            (symbol.Name, this._currentTypeName!) ) );

                    return false;
                }
            }

            private void CheckNullableContext( MemberDeclarationSyntax member, SyntaxToken name )
            {
                var nullableContext = this.RunTimeCompilation.GetSemanticModel( member.SyntaxTree ).GetNullableContext( member.SpanStart );

                if ( (nullableContext & NullableContext.Enabled) != NullableContext.Enabled )
                {
                    this.Success = false;

                    this._diagnosticAdder.Report(
                        TemplatingDiagnosticDescriptors.TemplateMustBeInNullableContext.CreateDiagnostic(
                            name.GetLocation(),
                            name.Text ) );
                }

                foreach ( var trivia in member.DescendantNodes( descendIntoTrivia: true ).Where( t => t.Kind() == SyntaxKind.NullableDirectiveTrivia ) )
                {
                    if ( ((NullableDirectiveTriviaSyntax) trivia).SettingToken.Kind() != SyntaxKind.EnableKeyword )
                    {
                        this.Success = false;

                        this._diagnosticAdder.Report(
                            TemplatingDiagnosticDescriptors.TemplateMustBeInNullableContext.CreateDiagnostic(
                                trivia.GetLocation(),
                                name.Text ) );
                    }
                }
            }

            private new IEnumerable<MethodDeclarationSyntax> VisitMethodDeclaration( MethodDeclarationSyntax node )
            {
                var methodSymbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node );

                if ( methodSymbol == null || this.SymbolClassifier.GetTemplateInfo( methodSymbol ).IsNone )
                {
                    yield return (MethodDeclarationSyntax) base.VisitMethodDeclaration( node ).AssertNotNull();

                    yield break;
                }

                if ( !this.CheckTemplateName( methodSymbol ) )
                {
                    yield break;
                }

                this.CheckNullableContext( node, node.Identifier );

                var success =
                    this._templateCompiler.TryCompile(
                        TemplateNameHelper.GetCompiledTemplateName( methodSymbol ),
                        this._compileTimeCompilation,
                        node,
                        this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ),
                        this._diagnosticAdder,
                        this._cancellationToken,
                        out _,
                        out var transformedNode );

                if ( success )
                {
                    if ( methodSymbol.IsOverride && methodSymbol.OverriddenMethod!.IsAbstract )
                    {
                        yield return this.WithThrowNotSupportedExceptionBody( node, "Template code cannot be directly executed." );
                    }
                    else
                    {
                        // The method can be deleted, i.e. it does not need to be inserted back in the member list.
                    }

                    yield return (MethodDeclarationSyntax) transformedNode.AssertNotNull();
                }
                else
                {
                    this.Success = false;
                }
            }

            private IEnumerable<MemberDeclarationSyntax> VisitBasePropertyDeclaration( BasePropertyDeclarationSyntax node )
            {
                var propertySymbol = (IPropertySymbol) this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node ).AssertNotNull();

                var propertyIsTemplate = !this.SymbolClassifier.GetTemplateInfo( propertySymbol ).IsNone;
                var propertyOrAccessorsAreTemplate = propertyIsTemplate;

                var success = true;
                SyntaxNode? transformedGetDeclaration = null;
                SyntaxNode? transformedSetDeclaration = null;

                // Compile accessors into templates.
                if ( !propertySymbol.IsAbstract )
                {
                    if ( node.AccessorList != null )
                    {
                        var templateAccessorCount = 0;

                        var getAccessor = node.AccessorList.Accessors.SingleOrDefault( a => a.Kind() == SyntaxKind.GetAccessorDeclaration );

                        var getterIsTemplate = getAccessor != null
                                               && (propertyIsTemplate || !this.SymbolClassifier.GetTemplateInfo( propertySymbol.GetMethod! ).IsNone);

                        var setAccessor = node.AccessorList.Accessors.SingleOrDefault(
                            a => a.Kind() == SyntaxKind.SetAccessorDeclaration || a.Kind() == SyntaxKind.InitAccessorDeclaration );

                        var setterIsTemplate = setAccessor != null
                                               && (propertyIsTemplate || !this.SymbolClassifier.GetTemplateInfo( propertySymbol.SetMethod! ).IsNone);

                        // Auto properties don't have bodies and so we don't need templates.

                        if ( getterIsTemplate && (getAccessor!.Body != null || getAccessor.ExpressionBody != null) )
                        {
                            success =
                                success &&
                                this._templateCompiler.TryCompile(
                                    TemplateNameHelper.GetCompiledTemplateName( propertySymbol.GetMethod.AssertNotNull() ),
                                    this._compileTimeCompilation,
                                    getAccessor,
                                    this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ),
                                    this._diagnosticAdder,
                                    this._cancellationToken,
                                    out _,
                                    out transformedGetDeclaration );

                            templateAccessorCount++;
                        }

                        if ( setterIsTemplate && (setAccessor!.Body != null || setAccessor.ExpressionBody != null) )
                        {
                            success =
                                success &&
                                this._templateCompiler.TryCompile(
                                    TemplateNameHelper.GetCompiledTemplateName( propertySymbol.SetMethod.AssertNotNull() ),
                                    this._compileTimeCompilation,
                                    setAccessor,
                                    this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ),
                                    this._diagnosticAdder,
                                    this._cancellationToken,
                                    out _,
                                    out transformedSetDeclaration );

                            templateAccessorCount++;
                        }

                        if ( templateAccessorCount > 0 )
                        {
                            propertyOrAccessorsAreTemplate = true;

                            if ( templateAccessorCount != node.AccessorList.Accessors.Count )
                            {
                                throw new AssertionFailedException( "When one accessor is a template, the other must also be a template." );
                            }
                        }
                    }
                    else if ( propertyIsTemplate && node is PropertyDeclarationSyntax { ExpressionBody: not null } propertyNode )
                    {
                        // Expression bodied property.
                        // TODO: Does this preserve trivia in expression body?
                        success =
                            success &&
                            this._templateCompiler.TryCompile(
                                TemplateNameHelper.GetCompiledTemplateName( propertySymbol.GetMethod.AssertNotNull() ),
                                this._compileTimeCompilation,
                                propertyNode,
                                this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ),
                                this._diagnosticAdder,
                                this._cancellationToken,
                                out _,
                                out transformedGetDeclaration );
                    }
                }

                if ( success )
                {
                    if ( !propertyOrAccessorsAreTemplate )
                    {
                        var suppressReadOnly = false;

                        if ( this._serializableFieldsAndProperties.TryGetValue( propertySymbol, out var serializableTypeInfo ) )
                        {
                            suppressReadOnly = this._serializerGenerator.ShouldSuppressReadOnly( serializableTypeInfo, propertySymbol );
                        }

                        var rewritten = (BasePropertyDeclarationSyntax) this.Visit( node ).AssertNotNull();

                        if ( suppressReadOnly && rewritten is PropertyDeclarationSyntax rewrittenProperty )
                        {
                            // If the property needs to have set accessor because of serialization, add it.
                            Invariant.Assert( rewrittenProperty.IsAutoPropertyDeclaration() );
                            Invariant.Assert( rewrittenProperty.AccessorList != null );

                            Invariant.Assert(
                                !rewrittenProperty.AccessorList!.Accessors.Any(
                                    a => a.IsKind( SyntaxKind.SetAccessorDeclaration ) || a.IsKind( SyntaxKind.InitAccessorDeclaration ) )
                                || rewrittenProperty.AccessorList!.Accessors.Any( a => a.IsKind( SyntaxKind.InitAccessorDeclaration ) ) );

                            rewritten =
                                rewrittenProperty.WithAccessorList(
                                    rewrittenProperty.AccessorList.WithAccessors(
                                        List(
                                            rewrittenProperty.AccessorList.Accessors
                                                .Where( a => !a.IsKind( SyntaxKind.InitAccessorDeclaration ) )
                                                .Append(
                                                    AccessorDeclaration(
                                                        SyntaxKind.SetAccessorDeclaration,
                                                        List<AttributeListSyntax>(),
                                                        TokenList( Token( SyntaxKind.PrivateKeyword ) ),
                                                        null,
                                                        null ) ) ) ) );
                        }

                        yield return rewritten;
                    }
                    else if ( propertySymbol.IsOverride && propertySymbol.OverriddenProperty!.IsAbstract )
                    {
                        // If the property implements an abstract property, it cannot be removed.

                        yield return this.WithThrowNotSupportedExceptionBody( node, "Template code cannot be directly executed." );
                    }
                    else
                    {
                        // The property can be deleted, i.e. it does not need to be inserted back in the member list.
                    }

                    if ( transformedGetDeclaration != null )
                    {
                        yield return (MemberDeclarationSyntax) transformedGetDeclaration;
                    }

                    if ( transformedSetDeclaration != null )
                    {
                        yield return (MemberDeclarationSyntax) transformedSetDeclaration;
                    }
                }
                else
                {
                    this.Success = false;
                }
            }

            private new IEnumerable<MemberDeclarationSyntax> VisitFieldDeclaration( FieldDeclarationSyntax node )
            {
                var unchangedVariables = new List<VariableDeclaratorSyntax>();
                var nonReadOnlyVariables = new List<VariableDeclaratorSyntax>();

                foreach ( var declarator in node.Declaration.Variables )
                {
                    var fieldSymbol = (IFieldSymbol) this.RunTimeCompilation.GetSemanticModel( declarator.SyntaxTree )
                        .GetDeclaredSymbol( declarator )
                        .AssertNotNull();

                    if ( this._serializableFieldsAndProperties.TryGetValue( fieldSymbol, out var serializableType )
                         && this._serializerGenerator.ShouldSuppressReadOnly( serializableType, fieldSymbol ) )
                    {
                        // This field needs to have it's readonly modifier removed.
                        nonReadOnlyVariables.Add( (VariableDeclaratorSyntax) this.Visit( declarator ).AssertNotNull() );
                    }
                    else
                    {
                        unchangedVariables.Add( (VariableDeclaratorSyntax) this.Visit( declarator ).AssertNotNull() );
                    }
                }

                if ( nonReadOnlyVariables.Count > 0 )
                {
                    if ( unchangedVariables.Count > 0 )
                    {
                        yield return
                            node.WithDeclaration( node.Declaration.WithVariables( SeparatedList( unchangedVariables ) ) );
                    }

                    yield return
                        node.WithDeclaration( node.Declaration.WithVariables( SeparatedList( nonReadOnlyVariables ) ) )
                            .WithModifiers( TokenList( node.Modifiers.Where( t => !t.IsKind( SyntaxKind.ReadOnlyKeyword ) ) ) );
                }
                else
                {
                    var visitedNode = this.Visit( node );

                    if ( visitedNode != null )
                    {
                        yield return (MemberDeclarationSyntax) visitedNode;
                    }
                }
            }

            private new IEnumerable<MemberDeclarationSyntax> VisitEventDeclaration( EventDeclarationSyntax node )
            {
                var eventSymbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetDeclaredSymbol( node ).AssertNotNull();

                if ( this.SymbolClassifier.GetTemplateInfo( eventSymbol ).IsNone )
                {
                    yield return (BasePropertyDeclarationSyntax) this.Visit( node ).AssertNotNull();

                    yield break;
                }

                if ( !this.CheckTemplateName( eventSymbol ) )
                {
                    yield break;
                }

                this.CheckNullableContext( node, node.Identifier );

                var success = true;
                SyntaxNode? transformedAddDeclaration = null;
                SyntaxNode? transformedRemoveDeclaration = null;

                // Compile accessors into templates.
                if ( !eventSymbol.IsAbstract )
                {
                    if ( node.AccessorList != null )
                    {
                        var addAccessor = node.AccessorList.Accessors.Single( a => a.Kind() == SyntaxKind.AddAccessorDeclaration );
                        var removeAccessor = node.AccessorList.Accessors.Single( a => a.Kind() == SyntaxKind.RemoveAccessorDeclaration );

                        success = success &&
                                  this._templateCompiler.TryCompile(
                                      TemplateNameHelper.GetCompiledTemplateName( eventSymbol.AddMethod.AssertNotNull() ),
                                      this._compileTimeCompilation,
                                      addAccessor,
                                      this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ),
                                      this._diagnosticAdder,
                                      this._cancellationToken,
                                      out _,
                                      out transformedAddDeclaration );

                        success = success &&
                                  this._templateCompiler.TryCompile(
                                      TemplateNameHelper.GetCompiledTemplateName( eventSymbol.RemoveMethod.AssertNotNull() ),
                                      this._compileTimeCompilation,
                                      removeAccessor,
                                      this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ),
                                      this._diagnosticAdder,
                                      this._cancellationToken,
                                      out _,
                                      out transformedRemoveDeclaration );
                    }
                }

                if ( success )
                {
                    yield return this.WithThrowNotSupportedExceptionBody( node, "Template code cannot be directly executed." );

                    if ( transformedAddDeclaration != null )
                    {
                        yield return (MemberDeclarationSyntax) transformedAddDeclaration;
                    }

                    if ( transformedRemoveDeclaration != null )
                    {
                        yield return (MemberDeclarationSyntax) transformedRemoveDeclaration;
                    }
                }
                else
                {
                    this.Success = false;
                }
            }

            private SyntaxList<MemberDeclarationSyntax> VisitTypeOrNamespaceMembers( SyntaxList<MemberDeclarationSyntax> members )
            {
                var resultingMembers = new List<MemberDeclarationSyntax>( members.Count );

                foreach ( var member in members )
                {
                    switch ( member )
                    {
                        case TypeDeclarationSyntax type:
                            resultingMembers.AddRange( this.VisitTypeDeclaration( type ) );

                            break;

                        default:
                            var transformedMember = (MemberDeclarationSyntax?) this.Visit( member );

                            if ( transformedMember != null )
                            {
                                resultingMembers.Add( transformedMember );
                            }

                            break;
                    }
                }

                return List( resultingMembers );
            }

            public override SyntaxNode? VisitNamespaceDeclaration( NamespaceDeclarationSyntax node )
            {
                var transformedMembers = this.VisitTypeOrNamespaceMembers( node.Members );

                if ( transformedMembers.Any( m => m.HasAnnotation( _hasCompileTimeCodeAnnotation ) ) )
                {
                    return node.WithMembers( transformedMembers ).WithAdditionalAnnotations( _hasCompileTimeCodeAnnotation );
                }
                else
                {
                    return null;
                }
            }

            public override SyntaxNode? VisitCompilationUnit( CompilationUnitSyntax node )
            {
                var transformedMembers = this.VisitTypeOrNamespaceMembers( node.Members );

                if ( transformedMembers.Any( m => m.HasAnnotation( _hasCompileTimeCodeAnnotation ) ) )
                {
                    return node.WithMembers( transformedMembers ).WithAdditionalAnnotations( _hasCompileTimeCodeAnnotation );
                }
                else
                {
                    // The rewriter should not have been invoked in a compilation unit that does not
                    // contain any build-time code. However, the compilation unit can contain only illegitimate compile-time
                    // code which has been stripped. In this case, we return an empty compilation unit.

                    return CompilationUnit( default, default, default, default );
                }
            }

            public override SyntaxNode? VisitInvocationExpression( InvocationExpressionSyntax node )
            {
                if ( this._currentContext.Scope != TemplatingScope.RunTimeOnly && node.IsNameOf() )
                {
                    var symbolInfo = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree )
                        .GetSymbolInfo( node.ArgumentList.Arguments[0].Expression );

                    var typeSymbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();

                    if ( typeSymbol != null )
                    {
                        return SyntaxFactoryEx.LiteralExpression( typeSymbol.Name );
                    }
                }

                return base.VisitInvocationExpression( node );
            }

            public override SyntaxNode? VisitTypeOfExpression( TypeOfExpressionSyntax node )
            {
                if ( this._currentContext.Scope != TemplatingScope.RunTimeOnly )
                {
                    if ( this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetSymbolInfo( node.Type ).Symbol is ITypeSymbol typeSymbol
                         && this.SymbolClassifier.GetTemplatingScope( typeSymbol ) == TemplatingScope.RunTimeOnly )
                    {
                        // We are in a compile-time-only block but we have a typeof to a run-time-only block. 
                        // This is a situation we can handle by rewriting the typeof to a call to UserCodeContext.GetCompileTimeType.

                        var memberAccess =
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                this._compileTimeTypeName,
                                IdentifierName( nameof(CompileTimeType.GetCompileTimeType) ) );

                        var invocation = InvocationExpression(
                            memberAccess,
                            ArgumentList(
                                SeparatedList(
                                    new[]
                                    {
                                        Argument( SyntaxFactoryEx.LiteralExpression( typeSymbol.GetSymbolId().ToString() ) ),
                                        Argument( SyntaxFactoryEx.LiteralExpression( typeSymbol.GetReflectionName() ) )
                                    } ) ) );

                        return invocation;
                    }
                }

                return base.VisitTypeOfExpression( node );
            }

            private T? AddLocationAnnotation<T>( T? originalNode, T? transformedNode )
                where T : SyntaxNode
                => originalNode == null || transformedNode == null
                    ? null
                    : (T?) this._templateCompiler.LocationAnnotationMap.AddLocationAnnotation( originalNode, transformedNode );

            // The default implementation of Visit(SyntaxNode) and Visit(SyntaxToken) adds the location annotations.

            public override SyntaxNode? Visit( SyntaxNode? node ) => this.AddLocationAnnotation( node, base.Visit( node ) );

            public override SyntaxToken VisitToken( SyntaxToken token ) => this._templateCompiler.LocationAnnotationMap.AddLocationAnnotation( token );

            public override SyntaxNode? VisitQualifiedName( QualifiedNameSyntax node )
            {
                // Fully qualify type names and namespaces.

                var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetSymbolInfo( node ).Symbol;

                if ( symbol is INamespaceOrTypeSymbol namespaceOrType )
                {
                    return OurSyntaxGenerator.CompileTime.NameExpression( namespaceOrType ).WithTriviaFrom( node );
                }

                return base.VisitQualifiedName( node );
            }

            public override SyntaxNode? VisitMemberAccessExpression( MemberAccessExpressionSyntax node )
            {
                // Fully qualify type names and namespaces.

                var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetSymbolInfo( node ).Symbol;

                if ( symbol is INamespaceOrTypeSymbol namespaceOrType )
                {
                    return OurSyntaxGenerator.CompileTime.NameExpression( namespaceOrType ).WithTriviaFrom( node );
                }

                return base.VisitMemberAccessExpression( node );
            }

            public override SyntaxNode? VisitIdentifierName( IdentifierNameSyntax node )
            {
                if ( node.Identifier.Text == "dynamic" )
                {
                    return PredefinedType( Token( SyntaxKind.ObjectKeyword ) ).WithTriviaFrom( node );
                }
                else if ( node.Identifier.Kind() == SyntaxKind.IdentifierToken && !node.IsVar )
                {
                    // Fully qualifies simple identifiers.

                    var symbol = this.RunTimeCompilation.GetSemanticModel( node.SyntaxTree ).GetSymbolInfo( node ).Symbol;

                    if ( symbol is INamespaceOrTypeSymbol namespaceOrType )
                    {
                        return OurSyntaxGenerator.CompileTime.NameExpression( namespaceOrType ).WithTriviaFrom( node );
                    }
                    else if ( symbol is { IsStatic: true } && node.Parent is not MemberAccessExpressionSyntax && node.Parent is not AliasQualifiedNameSyntax )
                    {
                        switch ( symbol.Kind )
                        {
                            case SymbolKind.Field:
                            case SymbolKind.Property:
                            case SymbolKind.Event:
                            case SymbolKind.Method:
                                // We have an access to a field or method with a "using static", or a non-qualified static member access.
                                return MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    OurSyntaxGenerator.CompileTime.NameExpression( symbol.ContainingType ),
                                    IdentifierName( node.Identifier.Text ) );
                        }
                    }
                }

                return base.VisitIdentifierName( node );
            }

            protected override T RewriteThrowNotSupported<T>( T node ) => ReplaceDynamicToObjectRewriter.Rewrite( node );

            private Context WithScope( TemplatingScope scope )
            {
                this._currentContext = new Context( scope, this );

                return this._currentContext;
            }

            // TODO: top-level statements?

            private class Context : IDisposable
            {
                private readonly ProduceCompileTimeCodeRewriter _parent;
                private readonly Context _oldContext;

                public Context( TemplatingScope scope, ProduceCompileTimeCodeRewriter parent )
                {
                    this.Scope = scope;
                    this._parent = parent;

                    // This will be null for the root context.
                    this._oldContext = parent._currentContext;
                }

                public TemplatingScope Scope { get; }

                public void Dispose() => this._parent._currentContext = this._oldContext;
            }
        }
    }
}