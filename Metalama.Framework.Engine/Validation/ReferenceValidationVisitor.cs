// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Project;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Threading;

namespace Metalama.Framework.Engine.Validation;

public class ReferenceValidationVisitor : SafeSyntaxWalker, IDisposable
{
    private const int _initialStackSize = 8;
    private readonly IDiagnosticSink _diagnosticAdder;
    private readonly Func<ISymbol, ImmutableArray<ReferenceValidatorInstance>> _getValidatorsFunc;
    private readonly CompilationModel _compilation;
    private readonly UserCodeInvoker _userCodeInvoker;
    private readonly CancellationToken _cancellationToken;
    private readonly UserCodeExecutionContext _userCodeExecutionContext;
    private readonly DisposeAction _disposeExecutionContext;
    private SemanticModel? _semanticModel;
    private int _stackIndex = -1;
    private SyntaxNode?[] _nodeStack = new SyntaxNode?[_initialStackSize];
    private IDeclaration?[] _declarationStack = new IDeclaration?[_initialStackSize];

    public ReferenceValidationVisitor(
        IServiceProvider serviceProvider,
        UserDiagnosticSink diagnosticAdder,
        Func<ISymbol, ImmutableArray<ReferenceValidatorInstance>> getValidatorsFunc,
        CompilationModel compilation,
        CancellationToken cancellationToken )
    {
        this._diagnosticAdder = diagnosticAdder;
        this._getValidatorsFunc = getValidatorsFunc;
        this._compilation = compilation;
        this._userCodeInvoker = serviceProvider.GetRequiredService<UserCodeInvoker>();
        this._userCodeExecutionContext = new UserCodeExecutionContext( serviceProvider, diagnosticAdder, default, compilationModel: compilation );
        this._cancellationToken = cancellationToken;
        this._disposeExecutionContext = UserCodeExecutionContext.WithContext( this._userCodeExecutionContext );
    }

    public void Visit( SyntaxTree syntaxTree )
    {
        this._semanticModel = this._compilation.RoslynCompilation.GetSemanticModel( syntaxTree );
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
        this.ValidateSymbol( node, ReferenceKinds.Other );
    }

    public override void VisitAssignmentExpression( AssignmentExpressionSyntax node )
    {
        this.Visit( node.Right );

        if ( !this.ValidateSymbol( node.Left, ReferenceKinds.Assignment ) )
        {
            this.Visit( node.Left );
        }
    }

    public override void VisitInvocationExpression( InvocationExpressionSyntax node )
    {
        this.ValidateSymbol( node.Expression, ReferenceKinds.Invocation );

        foreach ( var arg in node.ArgumentList.Arguments )
        {
            this.Visit( arg );
        }
    }

    public override void VisitBaseList( BaseListSyntax node )
    {
        foreach ( var baseType in node.Types )
        {
            this.VisitTypeReference( baseType.Type, ReferenceKinds.BaseType );
        }
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
        this.ValidateSymbol( node.Name, ReferenceKinds.AttributeType );

        if ( node.ArgumentList != null )
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

    public override void VisitSimpleBaseType( SimpleBaseTypeSyntax node )
    {
        this.VisitTypeReference( node.Type, ReferenceKinds.Other );
    }

    public override void VisitClassDeclaration( ClassDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            base.VisitClassDeclaration( node );
        }
    }

    public override void VisitRecordDeclaration( RecordDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            base.VisitRecordDeclaration( node );
        }
    }

    public override void VisitStructDeclaration( StructDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            base.VisitStructDeclaration( node );
        }
    }

    public override void VisitDelegateDeclaration( DelegateDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            base.VisitDelegateDeclaration( node );
        }
    }

    public override void VisitEnumDeclaration( EnumDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            base.VisitEnumDeclaration( node );
        }
    }

    public override void VisitMethodDeclaration( MethodDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            this.VisitTypeReference( node.ReturnType, ReferenceKinds.ReturnType );

            foreach ( var parameter in node.ParameterList.Parameters )
            {
                this.Visit( parameter );
            }

            this.Visit( node.AttributeLists );

            this.Visit( node.ExpressionBody );
            this.Visit( node.Body );
        }
    }

    public override void VisitPropertyDeclaration( PropertyDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            base.VisitPropertyDeclaration( node );
        }
    }

    public override void VisitEventDeclaration( EventDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            base.VisitEventDeclaration( node );
        }
    }

    public override void VisitFieldDeclaration( FieldDeclarationSyntax node )
    {
        using ( this.EnterContext( node.Declaration.Variables[0] ) )
        {
            this.VisitTypeReference( node.Declaration.Type, ReferenceKinds.FieldType );
        }

        foreach ( var field in node.Declaration.Variables )
        {
            if ( field.Initializer != null )
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
            base.VisitOperatorDeclaration( node );
        }
    }

    public override void VisitAccessorDeclaration( AccessorDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            base.VisitAccessorDeclaration( node );
        }
    }

    public override void VisitConstructorDeclaration( ConstructorDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            base.VisitConstructorDeclaration( node );
        }
    }

    public override void VisitDestructorDeclaration( DestructorDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            base.VisitDestructorDeclaration( node );
        }
    }

    public override void VisitConversionOperatorDeclaration( ConversionOperatorDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            base.VisitConversionOperatorDeclaration( node );
        }
    }

    public override void VisitIndexerDeclaration( IndexerDeclarationSyntax node )
    {
        using ( this.EnterContext( node ) )
        {
            base.VisitIndexerDeclaration( node );
        }
    }

    public override void VisitObjectCreationExpression( ObjectCreationExpressionSyntax node )
    {
        this.ValidateSymbol( node, ReferenceKinds.ObjectCreation );
        base.VisitObjectCreationExpression( node );
    }

    public override void VisitImplicitObjectCreationExpression( ImplicitObjectCreationExpressionSyntax node )
    {
        this.ValidateSymbol( node, ReferenceKinds.ObjectCreation );
        base.VisitImplicitObjectCreationExpression( node );
    }

    private bool ValidateSymbol( SyntaxNode? node, ReferenceKinds referenceKind )
    {
        if ( node == null )
        {
            return false;
        }

        var symbol = this._semanticModel!.GetSymbolInfo( node ).Symbol;

        return this.ValidateSymbol( node, symbol, referenceKind );
    }

    private bool ValidateSymbol( SyntaxNode node, ISymbol? symbol, ReferenceKinds referenceKinds )
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

        var validators = this._getValidatorsFunc( symbol );

        foreach ( var validator in validators )
        {
            if ( (validator.ReferenceKinds & referenceKinds) != 0 )
            {
                this._userCodeExecutionContext.InvokedMember = validator.Driver.UserCodeMemberInfo;
                validator.Validate( currentDeclaration, node, referenceKinds, this._diagnosticAdder, this._userCodeInvoker, this._userCodeExecutionContext );
            }
        }

        if ( symbol.ContainingType != null )
        {
            this.ValidateSymbol( node, symbol.ContainingType, referenceKinds );
        }
        else if ( symbol.ContainingNamespace != null )
        {
            this.ValidateSymbol( node, symbol.ContainingNamespace, referenceKinds );
        }

        return true;
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
                this.ValidateSymbol( type, kind );

                break;

            case SyntaxKind.NullableType:
                this.ValidateSymbol( ((NullableTypeSyntax) type).ElementType, kind | ReferenceKinds.NullableType );

                break;

            case SyntaxKind.ArrayType:
                this.ValidateSymbol( ((ArrayTypeSyntax) type).ElementType, kind | ReferenceKinds.ArrayType );

                break;

            case SyntaxKind.PointerType:
                this.ValidateSymbol( ((PointerTypeSyntax) type).ElementType, kind | ReferenceKinds.PointerType );

                break;

            case SyntaxKind.RefType:
                this.ValidateSymbol( ((RefTypeSyntax) type).Type, kind | ReferenceKinds.RefType );

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
                        this.ValidateSymbol( genericType, ((INamedTypeSymbol) symbol).ConstructedFrom, kind );
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