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
    private readonly IReferenceIndexerOptions _options;

    private SemanticModel? _semanticModel;
    private ISymbol? _currentDeclaration;

    public ReferenceIndexWalker(
        ProjectServiceProvider serviceProvider,
        CancellationToken cancellationToken,
        ReferenceIndexBuilder referenceIndexBuilder,
        IReferenceIndexerOptions options )
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
        this.ReferenceNodeAndChildren( node, ReferenceKinds.Default );
    }

    public override void VisitGenericName( GenericNameSyntax node )
    {
        this.ValidateNodeWithoutChildren( node, ReferenceKinds.Default, node.Identifier );
        this.Visit( node.TypeArgumentList );
    }

    public override void VisitElementAccessExpression( ElementAccessExpressionSyntax node )
    {
        this.ReferenceNodeAndChildren( node, ReferenceKinds.Default );
    }

    public override void VisitAssignmentExpression( AssignmentExpressionSyntax node )
    {
        this.Visit( node.Right );

        var symbol = this._semanticModel!.GetSymbolInfo( node.Left ).Symbol;

        if ( symbol != null )
        {
            this.ReferenceSymbol( symbol, node.Left, ReferenceKinds.Assignment );

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
        else
        {
            this.VisitChildren( node.Left );
        }
    }

    private void VisitChildren( SyntaxNode node )
    {
        foreach ( var nodeOrToken in node.ChildNodesAndTokens() )
        {
            if ( nodeOrToken.IsNode )
            {
                this.Visit( nodeOrToken.AsNode() );
            }
        }
    }

    public override void VisitInvocationExpression( InvocationExpressionSyntax node )
    {
        if ( node.IsNameOf() )
        {
            this.ReferenceNodeAndChildren( node.ArgumentList.Arguments[0].Expression, ReferenceKinds.NameOf );
        }
        else
        {
            this.ReferenceNodeAndChildren( node.Expression, ReferenceKinds.Invocation );

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
        this.ReferenceNodeAndChildren( node.Name, ReferenceKinds.AttributeType );

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
            this.ReferenceNodeAndChildren( node, ReferenceKinds.Default );
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
            var symbol = this._semanticModel.GetDeclaredSymbol( node );
            this.ReferenceSymbol( symbol?.OverriddenMethod, node, ReferenceKinds.OverrideMember );
            this.ValidateSymbols( node, symbol?.ExplicitInterfaceImplementations ?? default, ReferenceKinds.InterfaceMemberImplementation );

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

            var symbol = this._semanticModel.GetDeclaredSymbol( node );
            this.ReferenceSymbol( symbol?.OverriddenProperty, node, ReferenceKinds.OverrideMember );
            this.ValidateSymbols( node, symbol?.ExplicitInterfaceImplementations ?? default, ReferenceKinds.InterfaceMemberImplementation );
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

            var symbol = this._semanticModel.GetDeclaredSymbol( node );
            this.ReferenceSymbol( symbol?.OverriddenEvent, node, ReferenceKinds.OverrideMember );
            this.ValidateSymbols( node, symbol?.ExplicitInterfaceImplementations ?? default, ReferenceKinds.InterfaceMemberImplementation );

            this.VisitTypeReference( node.Type, ReferenceKinds.MemberType );

            this.Visit( node.AccessorList );
        }
    }

    public override void VisitFieldDeclaration( FieldDeclarationSyntax node )
    {
        using ( this.EnterDefinition( node.Declaration.Variables[0] ) )
        {
            this.Visit( node.AttributeLists );
            this.VisitTypeReference( node.Declaration.Type, ReferenceKinds.MemberType );
        }

        foreach ( var field in node.Declaration.Variables )
        {
            if ( field.Initializer != null && this._options.MustDescendIntoImplementation() )
            {
                using ( this.EnterDefinition( field ) )
                {
                    this.Visit( field.Initializer );
                }
            }
        }
    }

    public override void VisitEventFieldDeclaration( EventFieldDeclarationSyntax node )
    {
        using ( this.EnterDefinition( node.Declaration.Variables[0] ) )
        {
            this.Visit( node.AttributeLists );
            this.VisitTypeReference( node.Declaration.Type, ReferenceKinds.MemberType );
        }

        foreach ( var field in node.Declaration.Variables )
        {
            if ( field.Initializer != null && this._options.MustDescendIntoImplementation() )
            {
                using ( this.EnterDefinition( field ) )
                {
                    this.Visit( field.Initializer );
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
            ISymbol? baseConstructorSymbol;
            SyntaxNodeOrToken baseConstructorNode;

            if ( node.Initializer != null )
            {
                baseConstructorSymbol = this._semanticModel.GetSymbolInfo( node.Initializer ).Symbol;
                baseConstructorNode = node.Initializer.ThisOrBaseKeyword;
            }
            else
            {
                var symbol = this._semanticModel.GetDeclaredSymbol( node );
                baseConstructorSymbol = symbol?.ContainingType.BaseType?.Constructors.FirstOrDefault( c => c.Parameters.Length == 0 );
                baseConstructorNode = node.Identifier;
            }

            if ( baseConstructorSymbol != null )
            {
                this.ReferenceSymbol( baseConstructorSymbol, baseConstructorNode, ReferenceKinds.BaseConstructor );
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
        this.ReferenceNodeAndChildren( node, ReferenceKinds.ObjectCreation );
    }

    public override void VisitImplicitObjectCreationExpression( ImplicitObjectCreationExpressionSyntax node )
    {
        this.ReferenceNodeAndChildren( node, ReferenceKinds.ObjectCreation );
    }

    public override void VisitUsingDirective( UsingDirectiveSyntax node )
    {
        this.ReferenceNodeAndChildren( node.Name, ReferenceKinds.Using );
    }

    public override void VisitNamespaceDeclaration( NamespaceDeclarationSyntax node )
    {
        this.Visit( node.Members );
    }

    public override void VisitFileScopedNamespaceDeclaration( FileScopedNamespaceDeclarationSyntax node )
    {
        this.Visit( node.Members );
    }

    private void ReferenceNodeAndChildren( SyntaxNode? node, ReferenceKinds referenceKind )
    {
        if ( node == null )
        {
            return;
        }

        var symbol = this._semanticModel!.GetSymbolInfo( node ).Symbol;

        this.ReferenceSymbol( symbol, node, referenceKind );
        this.VisitChildren( node );
    }

    private void ValidateNodeWithoutChildren( SyntaxNode? node, ReferenceKinds referenceKind, SyntaxNodeOrToken? nodeForDiagnostics = null )
    {
        if ( node == null )
        {
            return;
        }

        var symbol = this._semanticModel!.GetSymbolInfo( node ).Symbol;

        this.ReferenceSymbol( symbol, nodeForDiagnostics ?? node, referenceKind );
    }

    private void ValidateSymbols<T>( SyntaxNode node, ImmutableArray<T> symbols, ReferenceKinds referenceKinds )
        where T : ISymbol
    {
        if ( !symbols.IsDefaultOrEmpty )
        {
            foreach ( var symbol in symbols )
            {
                this.ReferenceSymbol( symbol, node, referenceKinds );
            }
        }
    }

    // Returns true if a diagnostic was reported for the symbol.
    private void ReferenceSymbol(
        ISymbol? symbol,
        SyntaxNodeOrToken node,
        ReferenceKinds referenceKinds,
        bool isBaseType = false,
        bool isContainingType = false )
    {
        if ( symbol == null || this._currentDeclaration == null )
        {
            return;
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
                // Supported.
                break;

            default:
                // Not referenced.
                return;
        }

        if ( referenceKinds == ReferenceKinds.None )
        {
            // This happens for standalone identifiers or for member access.

            referenceKinds = ReferenceKinds.Default;
        }

        this._referenceIndexBuilder.AddReference( symbol, this._currentDeclaration, node, referenceKinds );
    }

    private ContextCookie EnterDefinition( SyntaxNode node )
    {
        var declaration = this._semanticModel.AssertNotNull().GetDeclaredSymbol( node );
        var previousDeclaration = this._currentDeclaration;

        if ( declaration != null )
        {
            this._currentDeclaration = declaration;

            return new ContextCookie( this, previousDeclaration );
        }
        else
        {
            return default;
        }
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
                this.ReferenceNodeAndChildren( type, kind );

                break;

            case SyntaxKind.NullableType:
                this.ReferenceNodeAndChildren( ((NullableTypeSyntax) type).ElementType, kind | ReferenceKinds.NullableType );

                break;

            case SyntaxKind.ArrayType:
                this.ReferenceNodeAndChildren( ((ArrayTypeSyntax) type).ElementType, kind | ReferenceKinds.ArrayType );

                break;

            case SyntaxKind.PointerType:
                this.ReferenceNodeAndChildren( ((PointerTypeSyntax) type).ElementType, kind | ReferenceKinds.PointerType );

                break;

            case SyntaxKind.RefType:
                this.ReferenceNodeAndChildren( ((RefTypeSyntax) type).Type, kind | ReferenceKinds.RefType );

                break;

            case SyntaxKind.TupleType:
                foreach ( var item in ((TupleTypeSyntax) type).Elements )
                {
                    this.VisitTypeReference( item.Type, kind | ReferenceKinds.TupleType );
                }

                break;

            case SyntaxKind.AliasQualifiedName:
            case SyntaxKind.FunctionPointerType:
                // Not implemented;
                break;

            case SyntaxKind.GenericName:
                {
                    var genericType = (GenericNameSyntax) type;
                    var symbol = this._semanticModel.GetSymbolInfo( genericType ).Symbol;

                    if ( symbol != null )
                    {
                        this.ReferenceSymbol( ((INamedTypeSymbol) symbol).ConstructedFrom, genericType, kind );
                    }

                    foreach ( var arg in genericType.TypeArgumentList.Arguments )
                    {
                        this.VisitTypeReference( arg, kind | ReferenceKinds.TypeArgument );
                    }
                }

                break;
        }
    }

    private readonly struct ContextCookie : IDisposable
    {
        private readonly ReferenceIndexWalker? _parent;
        private readonly ISymbol? _previousDeclaration;

        public ContextCookie( ReferenceIndexWalker parent, ISymbol? previousDeclaration )
        {
            this._parent = parent;
            this._previousDeclaration = previousDeclaration;
        }

        public void Dispose()
        {
            if ( this._parent != null )
            {
                this._parent._currentDeclaration = this._previousDeclaration;
            }
        }
    }
}