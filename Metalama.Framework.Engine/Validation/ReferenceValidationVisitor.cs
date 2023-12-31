// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;

namespace Metalama.Framework.Engine.Validation;

public sealed class ReferenceValidationVisitor : SafeSyntaxWalker, IDisposable
{
    private const int _initialStackSize = 8;
    private readonly CountingDiagnosticSink _diagnosticAdder;
    private readonly IReferenceValidatorProvider _validatorProvider;
    private readonly CompilationModel _compilation;
    private readonly SemanticModelProvider _semanticModelProvider;
    private readonly UserCodeInvoker _userCodeInvoker;
    private readonly CancellationToken _cancellationToken;
    private readonly UserCodeExecutionContext _userCodeExecutionContext;
    private readonly DisposeAction _disposeExecutionContext;
    private readonly ISymbolClassificationService _symbolClassifier;
    private SemanticModel? _semanticModel;
    private int _stackIndex = -1;
    private SyntaxNode?[] _nodeStack = new SyntaxNode?[_initialStackSize];
    private IDeclaration?[] _declarationStack = new IDeclaration?[_initialStackSize];

    public ReferenceValidationVisitor(
        ProjectServiceProvider serviceProvider,
        UserDiagnosticSink diagnosticAdder,
        IReferenceValidatorProvider validatorProvider,
        CompilationModel compilation,
        CancellationToken cancellationToken )
    {
        // This class cannot run concurrently on many threads.
        this._diagnosticAdder = new CountingDiagnosticSink( diagnosticAdder );
        this._validatorProvider = validatorProvider;
        this._compilation = compilation;
        this._semanticModelProvider = compilation.RoslynCompilation.GetSemanticModelProvider();
        this._userCodeInvoker = serviceProvider.GetRequiredService<UserCodeInvoker>();
        this._userCodeExecutionContext = new UserCodeExecutionContext( serviceProvider, diagnosticAdder, default, compilationModel: compilation );
        this._cancellationToken = cancellationToken;
        this._disposeExecutionContext = UserCodeExecutionContext.WithContext( this._userCodeExecutionContext );
        this._symbolClassifier = serviceProvider.GetRequiredService<ISymbolClassificationService>();
    }

    internal void Visit( SyntaxTree syntaxTree )
    {
        this._semanticModel = this._semanticModelProvider.GetSemanticModel( syntaxTree );
        this.Visit( syntaxTree.GetRoot() );
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
        this.ValidateNodeAndChildren( node, ReferenceKinds.Default );
    }

    public override void VisitGenericName( GenericNameSyntax node )
    {
        this.ValidateNodeWithoutChildren( node, ReferenceKinds.Default, node.Identifier );
        this.Visit( node.TypeArgumentList );
    }

    public override void VisitElementAccessExpression( ElementAccessExpressionSyntax node )
    {
        this.ValidateNodeAndChildren( node, ReferenceKinds.Default );
    }

    public override void VisitAssignmentExpression( AssignmentExpressionSyntax node )
    {
        this.Visit( node.Right );

        var symbol = this._semanticModel!.GetSymbolInfo( node.Left ).Symbol;

        if ( symbol != null )
        {
            if ( !this.ValidateSymbol( symbol, node.Left, ReferenceKinds.Assignment ) )
            {
                // If we have no report on the assignment itself, we still need to analyze the children of the left
                // part with a default ReferenceKind. However we cannot analyze the rightmost member of the expression
                // because this one is being assigned.
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
            this.ValidateNodeAndChildren( node.ArgumentList.Arguments[0].Expression, ReferenceKinds.NameOf );
        }
        else
        {
            this.ValidateNodeAndChildren( node.Expression, ReferenceKinds.Invocation );

            foreach ( var arg in node.ArgumentList.Arguments )
            {
                this.Visit( arg );
            }
        }
    }

    public override void VisitPrimaryConstructorBaseType( PrimaryConstructorBaseTypeSyntax node )
    {
        this.VisitTypeReference( node.Type, ReferenceKinds.BaseType );

        if ( this._validatorProvider.Properties.MustDescendIntoImplementation() )
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
        using ( this.EnterContext( node ) )
        {
            this.VisitTypeReference( node.Type, ReferenceKinds.ParameterType );
            this.Visit( node.AttributeLists );
        }
    }

    public override void VisitAttribute( AttributeSyntax node )
    {
        this.ValidateNodeAndChildren( node.Name, ReferenceKinds.AttributeType );

        if ( node.ArgumentList != null && this._validatorProvider.Properties.MustDescendIntoImplementation() )
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
        if ( this._validatorProvider.Properties.MustDescendIntoMembers() )
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

        using ( this.EnterContext( node ) )
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

        using ( this.EnterContext( node ) )
        {
            this.Visit( node.AttributeLists );
            this.Visit( node.BaseList );
            this.Visit( node.ConstraintClauses );

            if ( this._validatorProvider.Properties.MustDescendIntoMembers() )
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

        using ( this.EnterContext( node ) )
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

        using ( this.EnterContext( node ) )
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

        using ( this.EnterContext( node ) )
        {
            this.ValidateNodeAndChildren( node, ReferenceKinds.Default );
            this.Visit( node.AttributeLists );

            if ( this._validatorProvider.Properties.MustDescendIntoMembers() )
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
        using ( this.EnterContext( node ) )
        {
            var symbol = this._semanticModel.GetDeclaredSymbol( node );
            this.ValidateSymbol( symbol?.OverriddenMethod, node, ReferenceKinds.OverrideMember );
            this.ValidateSymbols( node, symbol?.ExplicitInterfaceImplementations ?? default, ReferenceKinds.InterfaceMemberImplementation );

            this.VisitTypeReference( node.ReturnType, ReferenceKinds.ReturnType );

            foreach ( var parameter in node.ParameterList.Parameters )
            {
                this.Visit( parameter );
            }

            this.Visit( node.AttributeLists );

            if ( this._validatorProvider.Properties.MustDescendIntoImplementation() )
            {
                this.Visit( node.ExpressionBody );
                this.Visit( node.Body );
            }
        }
    }

    public override void VisitPropertyDeclaration( PropertyDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            this.Visit( node.AttributeLists );

            var symbol = this._semanticModel.GetDeclaredSymbol( node );
            this.ValidateSymbol( symbol?.OverriddenProperty, node, ReferenceKinds.OverrideMember );
            this.ValidateSymbols( node, symbol?.ExplicitInterfaceImplementations ?? default, ReferenceKinds.InterfaceMemberImplementation );
            this.VisitTypeReference( node.Type, ReferenceKinds.MemberType );

            this.Visit( node.AccessorList );

            if ( this._validatorProvider.Properties.MustDescendIntoImplementation() )
            {
                this.Visit( node.ExpressionBody );
                this.Visit( node.Initializer );
            }
        }
    }

    public override void VisitEventDeclaration( EventDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            this.Visit( node.AttributeLists );

            var symbol = this._semanticModel.GetDeclaredSymbol( node );
            this.ValidateSymbol( symbol?.OverriddenEvent, node, ReferenceKinds.OverrideMember );
            this.ValidateSymbols( node, symbol?.ExplicitInterfaceImplementations ?? default, ReferenceKinds.InterfaceMemberImplementation );

            this.VisitTypeReference( node.Type, ReferenceKinds.MemberType );

            this.Visit( node.AccessorList );
        }
    }

    public override void VisitFieldDeclaration( FieldDeclarationSyntax node )
    {
        using ( this.EnterContext( node.Declaration.Variables[0] ) )
        {
            this.Visit( node.AttributeLists );
            this.VisitTypeReference( node.Declaration.Type, ReferenceKinds.MemberType );
        }

        foreach ( var field in node.Declaration.Variables )
        {
            if ( field.Initializer != null && this._validatorProvider.Properties.MustDescendIntoImplementation() )
            {
                using ( this.EnterContext( field ) )
                {
                    this.Visit( field.Initializer );
                }
            }
        }
    }

    public override void VisitEventFieldDeclaration( EventFieldDeclarationSyntax node )
    {
        using ( this.EnterContext( node.Declaration.Variables[0] ) )
        {
            this.Visit( node.AttributeLists );
            this.VisitTypeReference( node.Declaration.Type, ReferenceKinds.MemberType );
        }

        foreach ( var field in node.Declaration.Variables )
        {
            if ( field.Initializer != null && this._validatorProvider.Properties.MustDescendIntoImplementation() )
            {
                using ( this.EnterContext( field ) )
                {
                    this.Visit( field.Initializer );
                }
            }
        }
    }

    public override void VisitLocalDeclarationStatement( LocalDeclarationStatementSyntax node )
    {
        using ( this.EnterContext( node.Declaration.Variables[0] ) )
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
    }

    public override void VisitOperatorDeclaration( OperatorDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            this.VisitTypeReference( node.ReturnType, ReferenceKinds.ReturnType );

            foreach ( var parameter in node.ParameterList.Parameters )
            {
                this.Visit( parameter );
            }

            this.Visit( node.AttributeLists );

            if ( this._validatorProvider.Properties.MustDescendIntoImplementation() )
            {
                this.Visit( node.ExpressionBody );
                this.Visit( node.Body );
            }
        }
    }

    public override void VisitAccessorDeclaration( AccessorDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            this.Visit( node.AttributeLists );

            if ( this._validatorProvider.Properties.MustDescendIntoImplementation() )
            {
                this.Visit( node.ExpressionBody );
                this.Visit( node.Body );
            }
        }
    }

    public override void VisitConstructorDeclaration( ConstructorDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
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
                this.ValidateSymbol( baseConstructorSymbol, baseConstructorNode, ReferenceKinds.BaseConstructor );
            }

            // Visit parameters.
            foreach ( var parameter in node.ParameterList.Parameters )
            {
                this.Visit( parameter );
            }

            // Visit the body.
            if ( this._validatorProvider.Properties.MustDescendIntoImplementation() )
            {
                this.Visit( node.ExpressionBody );
                this.Visit( node.Body );
            }
        }
    }

    public override void VisitDestructorDeclaration( DestructorDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            this.Visit( node.AttributeLists );

            if ( this._validatorProvider.Properties.MustDescendIntoImplementation() )
            {
                this.Visit( node.ExpressionBody );
                this.Visit( node.Body );
            }
        }
    }

    public override void VisitConversionOperatorDeclaration( ConversionOperatorDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            this.VisitTypeReference( node.Type, ReferenceKinds.ReturnType );

            foreach ( var parameter in node.ParameterList.Parameters )
            {
                this.Visit( parameter );
            }

            this.Visit( node.AttributeLists );

            if ( this._validatorProvider.Properties.MustDescendIntoImplementation() )
            {
                this.Visit( node.ExpressionBody );
                this.Visit( node.Body );
            }
        }
    }

    public override void VisitIndexerDeclaration( IndexerDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            this.VisitTypeReference( node.Type, ReferenceKinds.ReturnType );

            foreach ( var parameter in node.ParameterList.Parameters )
            {
                this.Visit( parameter );
            }

            this.Visit( node.AttributeLists );

            if ( this._validatorProvider.Properties.MustDescendIntoImplementation() )
            {
                this.Visit( node.ExpressionBody );
            }

            this.Visit( node.AccessorList );
        }
    }

    public override void VisitObjectCreationExpression( ObjectCreationExpressionSyntax node )
    {
        this.ValidateNodeAndChildren( node, ReferenceKinds.ObjectCreation );
    }

    public override void VisitImplicitObjectCreationExpression( ImplicitObjectCreationExpressionSyntax node )
    {
        this.ValidateNodeAndChildren( node, ReferenceKinds.ObjectCreation );
    }

    public override void VisitUsingDirective( UsingDirectiveSyntax node )
    {
        this.ValidateNodeAndChildren( node.Name, ReferenceKinds.Using );
    }

    public override void VisitNamespaceDeclaration( NamespaceDeclarationSyntax node )
    {
        this.Visit( node.Members );
    }

    public override void VisitFileScopedNamespaceDeclaration( FileScopedNamespaceDeclarationSyntax node )
    {
        this.Visit( node.Members );
    }

    private void ValidateNodeAndChildren( SyntaxNode? node, ReferenceKinds referenceKind )
    {
        if ( node == null )
        {
            return;
        }

        var symbol = this._semanticModel!.GetSymbolInfo( node ).Symbol;

        if ( !this.ValidateSymbol( symbol, node, referenceKind ) )
        {
            this.VisitChildren( node );
        }
        else
        {
            // There were reports on the symbol itself so we don't go to children to avoid confusion and duplicates.
        }
    }

    private void ValidateNodeWithoutChildren( SyntaxNode? node, ReferenceKinds referenceKind, SyntaxNodeOrToken? nodeForDiagnostics = null )
    {
        if ( node == null )
        {
            return;
        }

        var symbol = this._semanticModel!.GetSymbolInfo( node ).Symbol;

        this.ValidateSymbol( symbol, nodeForDiagnostics ?? node, referenceKind );
    }

    private void ValidateSymbols<T>( SyntaxNode node, ImmutableArray<T> symbols, ReferenceKinds referenceKinds )
        where T : ISymbol
    {
        if ( !symbols.IsDefaultOrEmpty )
        {
            foreach ( var symbol in symbols )
            {
                this.ValidateSymbol( symbol, node, referenceKinds );
            }
        }
    }

    // Returns true if a diagnostic was reported for the symbol.
    private bool ValidateSymbol(
        ISymbol? symbol,
        SyntaxNodeOrToken node,
        ReferenceKinds referenceKinds,
        bool isBaseType = false,
        bool isContainingType = false )
    {
        if ( symbol == null || symbol.Kind == SymbolKind.Discard )
        {
            return false;
        }

        if ( referenceKinds == ReferenceKinds.None )
        {
            // This happens for standalone identifiers or for member access.

            referenceKinds = ReferenceKinds.Default;
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
                // Unsupported.
                return false;
        }

        var currentDeclaration = this.GetCurrentDeclaration();

        if ( currentDeclaration == null )
        {
            return false;
        }

        var validators = this._validatorProvider.GetValidators( symbol );

        var diagnosticsCountBefore = this._diagnosticAdder.DiagnosticCount;

        foreach ( var validator in validators )
        {
            if ( (validator.ReferenceKinds & referenceKinds) == 0 )
            {
                continue;
            }

            if ( isBaseType && !validator.IncludeDerivedTypes )
            {
                continue;
            }

            this._userCodeExecutionContext.Description = validator.Driver.GetUserCodeMemberInfo( validator );
            validator.Validate( currentDeclaration, node, referenceKinds, this._diagnosticAdder, this._userCodeInvoker, this._userCodeExecutionContext );
        }

        if ( symbol.ContainingType != null && this._validatorProvider.Properties.MustDescendIntoReferencedDeclaringType( referenceKinds ) )
        {
            this.ValidateSymbol( symbol.ContainingType, node, referenceKinds, isBaseType, true );
        }
        else if ( !isContainingType )
        {
            if ( symbol is { ContainingNamespace: not null } and { Kind: not SymbolKind.Namespace }
                 && this._validatorProvider.Properties.MustDescendIntoReferencedNamespace( referenceKinds ) )
            {
                // We validate namespaces, but not recursively because it is more cost-efficient when the user registers validators for all child namespaces.
                this.ValidateSymbol( symbol.ContainingNamespace, node, referenceKinds, isBaseType );
            }
            else if ( symbol.ContainingAssembly != null && this._validatorProvider.Properties.MustDescendIntoReferencedAssembly( referenceKinds ) )
            {
                this.ValidateSymbol( symbol.ContainingAssembly, node, referenceKinds, isBaseType );
            }
        }

        if ( symbol.Kind == SymbolKind.NamedType && this._validatorProvider.Properties.MustDescendIntoReferencedBaseTypes( referenceKinds ) )
        {
            var namedType = (INamedTypeSymbol) symbol;

            if ( namedType.BaseType != null )
            {
                this.ValidateSymbol( namedType.BaseType, node, referenceKinds, true );
            }

            foreach ( var i in namedType.Interfaces )
            {
                this.ValidateSymbol( i, node, referenceKinds, true );
            }
        }

        return this._diagnosticAdder.DiagnosticCount != diagnosticsCountBefore;
    }

    private IDeclaration? GetCurrentDeclaration()
    {
        for ( var i = this._stackIndex; i >= 0; i-- )
        {
            var declaredSymbol = this._semanticModel!.GetDeclaredSymbol( this._nodeStack[i]! );

            if ( declaredSymbol == null )
            {
                continue;
            }

            var declaration = this._declarationStack[i] ??= this._compilation.Factory.GetDeclarationOrNull( declaredSymbol );

            if ( declaration != null )
            {
                return declaration;
            }
        }

        return null;
    }

    private ContextCookie EnterContext( SyntaxNode node )
    {
        if ( this._nodeStack.Length < this._stackIndex + 2 )
        {
            Array.Resize( ref this._nodeStack, this._nodeStack.Length * 2 );
            Array.Resize( ref this._declarationStack, this._declarationStack.Length * 2 );
        }

        this._stackIndex++;
        this._nodeStack[this._stackIndex] = node;

        return new ContextCookie( this );
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
                this.ValidateNodeAndChildren( type, kind );

                break;

            case SyntaxKind.NullableType:
                this.ValidateNodeAndChildren( ((NullableTypeSyntax) type).ElementType, kind | ReferenceKinds.NullableType );

                break;

            case SyntaxKind.ArrayType:
                this.ValidateNodeAndChildren( ((ArrayTypeSyntax) type).ElementType, kind | ReferenceKinds.ArrayType );

                break;

            case SyntaxKind.PointerType:
                this.ValidateNodeAndChildren( ((PointerTypeSyntax) type).ElementType, kind | ReferenceKinds.PointerType );

                break;

            case SyntaxKind.RefType:
                this.ValidateNodeAndChildren( ((RefTypeSyntax) type).Type, kind | ReferenceKinds.RefType );

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
                        this.ValidateSymbol( ((INamedTypeSymbol) symbol).ConstructedFrom, genericType, kind );
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
        private readonly ReferenceValidationVisitor _parent;

        public ContextCookie( ReferenceValidationVisitor parent )
        {
            this._parent = parent;
        }

        public void Dispose()
        {
            this._parent._nodeStack[this._parent._stackIndex] = null;
            this._parent._declarationStack[this._parent._stackIndex] = null;
            this._parent._stackIndex--;
        }
    }

    public void Dispose()
    {
        this._disposeExecutionContext.Dispose();
    }
}