// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Validation;

internal sealed class ReferenceIndexWalker : SafeSyntaxWalker
{
    private readonly CancellationToken _cancellationToken;
    private readonly ISymbolClassificationService _symbolClassifier;
    private readonly ReferenceIndexBuilder _referenceIndexBuilder;
    private readonly ReferenceIndexerOptions _options;

    private SemanticModel? _semanticModel;
    private ISymbol? _currentDeclarationSymbol;
    private SyntaxNode? _currentDeclarationNode;
    private ReferenceKinds _currentReferenceKinds = ReferenceKinds.Default;

    public ReferenceIndexWalker(
        ProjectServiceProvider serviceProvider,
        CancellationToken cancellationToken,
        ReferenceIndexBuilder referenceIndexBuilder,
        ReferenceIndexerOptions options )
    {
        // This class cannot run concurrently on many threads.
        this._cancellationToken = cancellationToken;
        this._referenceIndexBuilder = referenceIndexBuilder;
        this._options = options;
        this._symbolClassifier = serviceProvider.GetRequiredService<ISymbolClassificationService>();
    }

    public void Visit( SemanticModel semanticModel )
    {
        this._semanticModel = semanticModel;
        this.Visit( semanticModel.SyntaxTree.GetRoot() );
    }

    public override void DefaultVisit( SyntaxNode node )
    {
        this._cancellationToken.ThrowIfCancellationRequested();
        base.DefaultVisit( node );
    }

    public override void VisitIdentifierName( IdentifierNameSyntax node )
    {
        this.IndexReference( node );
    }

    public override void VisitAssignmentExpression( AssignmentExpressionSyntax node )
    {
        this.Visit( node.Right );
        this.IndexReference( node.Left, ReferenceKinds.Assignment );

        switch ( node.Left )
        {
            case MemberAccessExpressionSyntax memberAccess:
                this.Visit( memberAccess.Expression );

                break;

            case ElementAccessExpressionSyntax elementAccess:
                this.Visit( elementAccess.Expression );
                this.Visit( elementAccess.ArgumentList );

                break;

            case IdentifierNameSyntax:
                // If we just have an identifier, we have nothing to visit.
                break;

            // Other cases are possible but we don't implement them.
            // For instance, we can assign the return value of a ref method.
        }
    }

    private bool MustSkipChildren( SyntaxNode node )
    {
        if ( node is MemberAccessExpressionSyntax memberAccess )
        {
            // Do not index MemberAccessExpression if the expression is a type, `this` or `base`.
            if ( memberAccess.Expression.Kind() is SyntaxKind.BaseExpression or SyntaxKind.ThisExpression
                 || this._semanticModel!.GetSymbolInfo( memberAccess ).Symbol is { IsStatic: true } )
            {
                return true;
            }
        }

        return false;
    }

    public override void VisitInvocationExpression( InvocationExpressionSyntax node )
    {
        if ( node.IsNameOf() )
        {
            this.VisitWithReferenceKinds( node.ArgumentList.Arguments[0].Expression, ReferenceKinds.NameOf );
        }
        else
        {
            this.VisitWithReferenceKinds( node.Expression, ReferenceKinds.Invocation );

            foreach ( var arg in node.ArgumentList.Arguments )
            {
                this.Visit( arg );
            }
        }
    }

    public override void VisitPrimaryConstructorBaseType( PrimaryConstructorBaseTypeSyntax node )
    {
        this.VisitTypeReference( node.Type, ReferenceKinds.BaseType );

        if ( this._options.MustDescendIntoImplementation() )
        {
            this.Visit( node.ArgumentList );
        }
    }

    public override void VisitSimpleBaseType( SimpleBaseTypeSyntax node )
    {
        this.VisitTypeReference( node.Type, ReferenceKinds.BaseType );
    }

    public override void VisitTypeArgumentList( TypeArgumentListSyntax node )
    {
        foreach ( var arg in node.Arguments )
        {
            this.VisitTypeReference( arg, ReferenceKinds.TypeArgument );
        }
    }

    public override void VisitTypeOfExpression( TypeOfExpressionSyntax node )
    {
        this.VisitTypeReference( node.Type, ReferenceKinds.TypeOf );
    }

    public override void VisitParameter( ParameterSyntax node )
    {
        using ( this.EnterDefinition( node ) )
        {
            this.VisitTypeReference( node.Type, ReferenceKinds.ParameterType );
            this.Visit( node.AttributeLists );
        }
    }

    public override void VisitAttribute( AttributeSyntax node )
    {
        this.VisitWithReferenceKinds( node.Name, ReferenceKinds.AttributeType );

        if ( node.ArgumentList != null && this._options.MustDescendIntoImplementation() )
        {
            foreach ( var arg in node.ArgumentList.Arguments )
            {
                this.Visit( arg );
            }
        }
    }

    public override void VisitTypeConstraint( TypeConstraintSyntax node )
    {
        this.VisitTypeReference( node.Type, ReferenceKinds.TypeConstraint );
    }

    private bool CanSkipTypeDeclaration( SyntaxNode node )
    {
        var symbol = this._semanticModel?.GetDeclaredSymbol( node );

        if ( symbol != null && this._symbolClassifier.GetExecutionScope( symbol ) != ExecutionScope.RunTime )
        {
            // We only validate run-time code.
            return true;
        }

        return false;
    }

    private void VisitMembers( SyntaxList<MemberDeclarationSyntax> members )
    {
        if ( this._options.MustDescendIntoMembers() )
        {
            this.Visit( members );
        }
        else
        {
            // Even if we must not descend into members, we must still visit nested types.
            foreach ( var member in members )
            {
                if ( SyntaxFacts.IsTypeDeclaration( member.Kind() ) )
                {
                    this.Visit( member );

                    break;
                }
            }
        }
    }

    public override void VisitClassDeclaration( ClassDeclarationSyntax node )
    {
        if ( this.CanSkipTypeDeclaration( node ) )
        {
            return;
        }

        using ( this.EnterDefinition( node ) )
        {
            this.Visit( node.AttributeLists );
            this.Visit( node.BaseList );
            this.Visit( node.ConstraintClauses );
            this.VisitMembers( node.Members );
        }
    }

    public override void VisitRecordDeclaration( RecordDeclarationSyntax node )
    {
        if ( this.CanSkipTypeDeclaration( node ) )
        {
            return;
        }

        using ( this.EnterDefinition( node ) )
        {
            this.Visit( node.AttributeLists );
            this.Visit( node.BaseList );
            this.Visit( node.ConstraintClauses );

            if ( this._options.MustDescendIntoMembers() )
            {
                this.Visit( node.ParameterList );
            }

            this.VisitMembers( node.Members );
        }
    }

    public override void VisitStructDeclaration( StructDeclarationSyntax node )
    {
        if ( this.CanSkipTypeDeclaration( node ) )
        {
            return;
        }

        using ( this.EnterDefinition( node ) )
        {
            this.Visit( node.AttributeLists );
            this.Visit( node.BaseList );
            this.Visit( node.ConstraintClauses );
            this.VisitMembers( node.Members );
        }
    }

    public override void VisitDelegateDeclaration( DelegateDeclarationSyntax node )
    {
        if ( this.CanSkipTypeDeclaration( node ) )
        {
            return;
        }

        using ( this.EnterDefinition( node ) )
        {
            base.VisitDelegateDeclaration( node );
        }
    }

    public override void VisitEnumDeclaration( EnumDeclarationSyntax node )
    {
        if ( this.CanSkipTypeDeclaration( node ) )
        {
            return;
        }

        using ( this.EnterDefinition( node ) )
        {
            this.Visit( node.BaseList );
            this.Visit( node.AttributeLists );

            if ( this._options.MustDescendIntoMembers() )
            {
                foreach ( var member in node.Members )
                {
                    this.Visit( member );
                }
            }
        }
    }

    public override void VisitMethodDeclaration( MethodDeclarationSyntax node )
    {
        using ( this.EnterDefinition( node ) )
        {
            this.IndexMember<IMethodSymbol>( node, m => m.OverriddenMethod, m => m.ExplicitInterfaceImplementations );

            this.VisitTypeReference( node.ReturnType, ReferenceKinds.ReturnType );

            foreach ( var parameter in node.ParameterList.Parameters )
            {
                this.Visit( parameter );
            }

            this.Visit( node.AttributeLists );

            if ( this._options.MustDescendIntoImplementation() )
            {
                this.Visit( node.ExpressionBody );
                this.Visit( node.Body );
            }
        }
    }

    public override void VisitPropertyDeclaration( PropertyDeclarationSyntax node )
    {
        using ( this.EnterDefinition( node ) )
        {
            this.Visit( node.AttributeLists );

            this.IndexMember<IPropertySymbol>( node, m => m.OverriddenProperty, m => m.ExplicitInterfaceImplementations );
            this.VisitTypeReference( node.Type, ReferenceKinds.MemberType );

            this.Visit( node.AccessorList );

            if ( this._options.MustDescendIntoImplementation() )
            {
                this.Visit( node.ExpressionBody );
                this.Visit( node.Initializer );
            }
        }
    }

    public override void VisitEventDeclaration( EventDeclarationSyntax node )
    {
        using ( this.EnterDefinition( node ) )
        {
            this.Visit( node.AttributeLists );

            this.IndexMember<IEventSymbol>( node, m => m.OverriddenEvent, m => m.ExplicitInterfaceImplementations );

            this.VisitTypeReference( node.Type, ReferenceKinds.MemberType );

            this.Visit( node.AccessorList );
        }
    }

    public override void VisitFieldDeclaration( FieldDeclarationSyntax node )
    {
        for ( var index = 0; index < node.Declaration.Variables.Count; index++ )
        {
            var variable = node.Declaration.Variables[index];

            using ( this.EnterDefinition( variable ) )
            {
                if ( index == 0 )
                {
                    // Attributes must be visited a single time.
                    this.Visit( node.AttributeLists );
                }

                // The type must be visited every time.
                this.VisitTypeReference( node.Declaration.Type, ReferenceKinds.MemberType );

                if ( variable.Initializer != null && this._options.MustDescendIntoImplementation() )
                {
                    using ( this.EnterDefinition( variable ) )
                    {
                        this.Visit( variable.Initializer );
                    }
                }
            }
        }
    }

    public override void VisitEventFieldDeclaration( EventFieldDeclarationSyntax node )
    {
        for ( var index = 0; index < node.Declaration.Variables.Count; index++ )
        {
            var variable = node.Declaration.Variables[index];

            using ( this.EnterDefinition( variable ) )
            {
                if ( index == 0 )
                {
                    // Attributes must be visited a single time.
                    this.Visit( node.AttributeLists );
                }

                // The type must be visited every time.
                this.VisitTypeReference( node.Declaration.Type, ReferenceKinds.MemberType );

                if ( variable.Initializer != null && this._options.MustDescendIntoImplementation() )
                {
                    using ( this.EnterDefinition( variable ) )
                    {
                        this.Visit( variable.Initializer );
                    }
                }
            }
        }
    }

    public override void VisitLocalDeclarationStatement( LocalDeclarationStatementSyntax node )
    {
        this.VisitTypeReference( node.Declaration.Type, ReferenceKinds.LocalVariableType );

        foreach ( var variable in node.Declaration.Variables )
        {
            if ( variable.Initializer != null )
            {
                this.Visit( variable.Initializer );
            }
        }
    }

    public override void VisitOperatorDeclaration( OperatorDeclarationSyntax node )
    {
        using ( this.EnterDefinition( node ) )
        {
            this.VisitTypeReference( node.ReturnType, ReferenceKinds.ReturnType );

            foreach ( var parameter in node.ParameterList.Parameters )
            {
                this.Visit( parameter );
            }

            this.Visit( node.AttributeLists );

            if ( this._options.MustDescendIntoImplementation() )
            {
                this.Visit( node.ExpressionBody );
                this.Visit( node.Body );
            }
        }
    }

    public override void VisitAccessorDeclaration( AccessorDeclarationSyntax node )
    {
        using ( this.EnterDefinition( node ) )
        {
            this.Visit( node.AttributeLists );

            if ( this._options.MustDescendIntoImplementation() )
            {
                this.Visit( node.ExpressionBody );
                this.Visit( node.Body );
            }
        }
    }

    public override void VisitConstructorDeclaration( ConstructorDeclarationSyntax node )
    {
        using ( this.EnterDefinition( node ) )
        {
            this.Visit( node.AttributeLists );

            // Visit the base constructor.
            if ( node.Initializer != null )
            {
                this.IndexReference( node.Initializer, node.Initializer.ThisOrBaseKeyword, ReferenceKinds.BaseConstructor );
            }
            else
            {
                // We need to find the base constructor.
                if ( this._options.MustIndexReferenceKind( ReferenceKinds.BaseConstructor ) )
                {
                    var symbol = this._semanticModel.GetDeclaredSymbol( node );
                    var baseConstructorSymbol = symbol?.ContainingType.BaseType?.Constructors.FirstOrDefault( c => c.Parameters.Length == 0 );

                    if ( baseConstructorSymbol != null )
                    {
                        this._referenceIndexBuilder.AddReference(
                            baseConstructorSymbol,
                            this.CurrentDeclarationSymbol.AssertNotNull(),
                            node.Identifier,
                            ReferenceKinds.BaseConstructor );
                    }
                }
            }

            // Visit parameters.
            foreach ( var parameter in node.ParameterList.Parameters )
            {
                this.Visit( parameter );
            }

            // Visit the body.
            if ( this._options.MustDescendIntoImplementation() )
            {
                this.Visit( node.ExpressionBody );
                this.Visit( node.Body );
            }
        }
    }

    public override void VisitDestructorDeclaration( DestructorDeclarationSyntax node )
    {
        using ( this.EnterDefinition( node ) )
        {
            this.Visit( node.AttributeLists );

            if ( this._options.MustDescendIntoImplementation() )
            {
                this.Visit( node.ExpressionBody );
                this.Visit( node.Body );
            }
        }
    }

    public override void VisitConversionOperatorDeclaration( ConversionOperatorDeclarationSyntax node )
    {
        using ( this.EnterDefinition( node ) )
        {
            this.VisitTypeReference( node.Type, ReferenceKinds.ReturnType );

            foreach ( var parameter in node.ParameterList.Parameters )
            {
                this.Visit( parameter );
            }

            this.Visit( node.AttributeLists );

            if ( this._options.MustDescendIntoImplementation() )
            {
                this.Visit( node.ExpressionBody );
                this.Visit( node.Body );
            }
        }
    }

    public override void VisitIndexerDeclaration( IndexerDeclarationSyntax node )
    {
        using ( this.EnterDefinition( node ) )
        {
            this.VisitTypeReference( node.Type, ReferenceKinds.ReturnType );

            foreach ( var parameter in node.ParameterList.Parameters )
            {
                this.Visit( parameter );
            }

            this.Visit( node.AttributeLists );

            if ( this._options.MustDescendIntoImplementation() )
            {
                this.Visit( node.ExpressionBody );
            }

            this.Visit( node.AccessorList );
        }
    }

    public override void VisitObjectCreationExpression( ObjectCreationExpressionSyntax node )
    {
        this.IndexReference( node, ReferenceKinds.ObjectCreation );

        this.Visit( node.ArgumentList );
        this.Visit( node.Initializer );
    }

    public override void VisitImplicitObjectCreationExpression( ImplicitObjectCreationExpressionSyntax node )
    {
        this.IndexReference( node, node.NewKeyword, ReferenceKinds.ObjectCreation );

        this.Visit( node.Initializer );
        this.Visit( node.ArgumentList );
    }

    public override void VisitUsingDirective( UsingDirectiveSyntax node )
    {
        this.VisitWithReferenceKinds( node.Name, ReferenceKinds.Using );
    }

    public override void VisitMemberAccessExpression( MemberAccessExpressionSyntax node )
    {
        this.Visit( node.Name );

        if ( !this.MustSkipChildren( node ) )
        {
            this.VisitWithReferenceKinds( node.Expression, ReferenceKinds.Default );
        }
    }

    public override void VisitNamespaceDeclaration( NamespaceDeclarationSyntax node )
    {
        this.Visit( node.Members );
    }

    public override void VisitFileScopedNamespaceDeclaration( FileScopedNamespaceDeclarationSyntax node )
    {
        this.Visit( node.Members );
    }

    public override void VisitElementAccessExpression( ElementAccessExpressionSyntax node )
    {
        // We may have an indexer access, which will not be discovered in another node than this one.
        this.IndexReference( node, node.ArgumentList );

        // Continue visiting the children.
        base.VisitElementAccessExpression( node );
    }

    private void VisitWithReferenceKinds( SyntaxNode? node, ReferenceKinds referenceKind )
    {
#if DEBUG
        if ( referenceKind == ReferenceKinds.None )
        {
            throw new ArgumentOutOfRangeException( nameof(referenceKind) );
        }
#endif

        if ( node == null )
        {
            return;
        }

        var previousReferenceKinds = this._currentReferenceKinds;
        this._currentReferenceKinds = referenceKind;
        this.Visit( node );
        this._currentReferenceKinds = previousReferenceKinds;
    }

    private void IndexReference(
        SyntaxNode node,
        ReferenceKinds referenceKinds = ReferenceKinds.Default )
        => this.IndexReference( node, node, referenceKinds );

    private void IndexReference(
        SyntaxNode nodeForSymbol,
        SyntaxNodeOrToken nodeForReference,
        ReferenceKinds referenceKinds = ReferenceKinds.Default )
    {
        if ( this._currentDeclarationNode == null )
        {
            return;
        }

        referenceKinds = this.GetEffectiveReferenceKind( referenceKinds );

        if ( this._options.MustIndexReferenceKind( referenceKinds ) )
        {
            var symbol = this._semanticModel!.GetSymbolInfo( nodeForSymbol ).Symbol;

            if ( !CanIndexSymbol( symbol ) )
            {
                return;
            }

            this._referenceIndexBuilder.AddReference( symbol!, this.CurrentDeclarationSymbol, nodeForReference, referenceKinds );
        }
    }

    private void IndexMember<T>(
        MemberDeclarationSyntax node,
        Func<T, T?> getOverridenMember,
        Func<T, ImmutableArray<T>> getImplementedInterfaceMembers )
        where T : class, ISymbol
    {
        if ( this._currentDeclarationNode == null )
        {
            return;
        }

        if ( this._options.MustIndexReferenceKind( ReferenceKinds.OverrideMember | ReferenceKinds.InterfaceMemberImplementation ) )
        {
            var symbol = (T?) this.CurrentDeclarationSymbol;

            if ( symbol == null )
            {
                return;
            }

            if ( this._options.MustIndexReferenceKind( ReferenceKinds.OverrideMember ) )
            {
                var overridenMember = getOverridenMember( symbol );

                if ( overridenMember != null )
                {
                    this._referenceIndexBuilder.AddReference( overridenMember, this.CurrentDeclarationSymbol, node, ReferenceKinds.OverrideMember );
                }
            }

            if ( this._options.MustIndexReferenceKind( ReferenceKinds.InterfaceMemberImplementation ) )
            {
                var interfaceMembers = getImplementedInterfaceMembers( symbol );

                foreach ( var member in interfaceMembers )
                {
                    this._referenceIndexBuilder.AddReference( member, this.CurrentDeclarationSymbol, node, ReferenceKinds.InterfaceMemberImplementation );
                }
            }
        }
    }

    private ReferenceKinds GetEffectiveReferenceKind( ReferenceKinds referenceKinds )
    {
        if ( referenceKinds is ReferenceKinds.Default or ReferenceKinds.None )
        {
            // This happens for standalone identifiers or for member access.
            referenceKinds = this._currentReferenceKinds;
        }

        return referenceKinds;
    }

    private static bool CanIndexSymbol( ISymbol? symbol )
    {
        if ( symbol == null )
        {
            return false;
        }

        switch ( symbol.Kind )
        {
            case SymbolKind.ArrayType:
            case SymbolKind.Assembly:
            case SymbolKind.DynamicType:
            case SymbolKind.Event:
            case SymbolKind.Field:
            case SymbolKind.Method:
            case SymbolKind.NetModule:
            case SymbolKind.NamedType:
            case SymbolKind.Namespace:
            case SymbolKind.Parameter:
            case SymbolKind.PointerType:
            case SymbolKind.Property:
            case SymbolKind.TypeParameter:
            case SymbolKind.FunctionPointerType:
                return true;

            default:
                // Not referenced.
                return false;
        }
    }

    private ISymbol CurrentDeclarationSymbol
        => this._currentDeclarationSymbol ??= this._currentDeclarationNode != null
            ? this._semanticModel!.GetDeclaredSymbol( this._currentDeclarationNode ).AssertNotNull()
            : throw new InvalidOperationException();

    private DeclarationContextCookie EnterDefinition( SyntaxNode node )
    {
        var previousSymbol = this._currentDeclarationSymbol;
        var previousNode = this._currentDeclarationNode;

        this._currentDeclarationSymbol = null;
        this._currentDeclarationNode = node;

        return new DeclarationContextCookie( this, previousSymbol, previousNode );
    }

    private void Visit<T>( SyntaxList<T> list )
        where T : SyntaxNode
    {
        foreach ( var node in list )
        {
            this.Visit( node );
        }
    }

    private void VisitTypeReference( TypeSyntax? type, ReferenceKinds kind )
    {
        if ( type == null )
        {
            return;
        }

        switch ( type.Kind() )
        {
            case SyntaxKind.IdentifierName:
            case SyntaxKind.QualifiedName:
            case SyntaxKind.PredefinedType:
                this.VisitWithReferenceKinds( type, kind );

                break;

            case SyntaxKind.NullableType:
                this.VisitWithReferenceKinds( ((NullableTypeSyntax) type).ElementType, kind );

                break;

            case SyntaxKind.ArrayType:
                this.VisitWithReferenceKinds( ((ArrayTypeSyntax) type).ElementType, ReferenceKinds.ArrayElementType );

                break;

            case SyntaxKind.PointerType:
                this.VisitWithReferenceKinds( ((PointerTypeSyntax) type).ElementType, ReferenceKinds.PointerType );

                break;

            case SyntaxKind.RefType:
                this.VisitWithReferenceKinds( ((RefTypeSyntax) type).Type, kind );

                break;

            case SyntaxKind.TupleType:
                foreach ( var item in ((TupleTypeSyntax) type).Elements )
                {
                    this.VisitTypeReference( item.Type, ReferenceKinds.TupleElementType );
                }

                break;

            case SyntaxKind.AliasQualifiedName:
            case SyntaxKind.FunctionPointerType:
                // Not implemented;
                break;

            case SyntaxKind.GenericName:
                {
                    var genericType = (GenericNameSyntax) type;

                    if ( genericType.Identifier.Text == nameof(Nullable<int>) )
                    {
                        // Process nullable types consitently as the ? operator.
                        this.IndexReference( genericType.TypeArgumentList.Arguments[0], kind );

                        return;
                    }
                    else
                    {
                        this.IndexReference( genericType, kind );
                    }

                    foreach ( var arg in genericType.TypeArgumentList.Arguments )
                    {
                        this.VisitTypeReference( arg, ReferenceKinds.TypeArgument );
                    }
                }

                break;
        }
    }

    private readonly struct DeclarationContextCookie : IDisposable
    {
        private readonly ReferenceIndexWalker? _parent;
        private readonly ISymbol? _previousDeclaration;
        private readonly SyntaxNode? _previousNode;

        public DeclarationContextCookie( ReferenceIndexWalker parent, ISymbol? previousDeclaration, SyntaxNode? previousNode )
        {
            this._parent = parent;
            this._previousDeclaration = previousDeclaration;
            this._previousNode = previousNode;
        }

        public void Dispose()
        {
            if ( this._parent != null )
            {
                this._parent._currentDeclarationSymbol = this._previousDeclaration;
                this._parent._currentDeclarationNode = this._previousNode;
            }
        }
    }
}