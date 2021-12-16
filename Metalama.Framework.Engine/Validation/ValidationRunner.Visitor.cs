// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.Validation;

internal partial class ValidationRunner
{
    private class Visitor : CSharpSyntaxWalker
    {
        private const int _initialStackSize = 8;
        private readonly IDiagnosticSink _diagnosticAdder;
        private readonly ImmutableDictionaryOfArray<ISymbol, ReferenceValidatorInstance> _validatorsBySymbol;
        private readonly CompilationModel _compilation;
        private SemanticModel? _semanticModel;
        private int _stackIndex = -1;
        private SyntaxNode?[] _nodeStack = new SyntaxNode?[_initialStackSize];
        private IDeclaration?[] _declarationStack = new IDeclaration?[_initialStackSize];

        public Visitor(
            IDiagnosticSink diagnosticAdder,
            ImmutableDictionaryOfArray<ISymbol, ReferenceValidatorInstance> validatorsBySymbol,
            CompilationModel compilation )
        {
            this._diagnosticAdder = diagnosticAdder;
            this._validatorsBySymbol = validatorsBySymbol;
            this._compilation = compilation;
        }

        public void Visit( SyntaxTree syntaxTree )
        {
            this._semanticModel = this._compilation.RoslynCompilation.GetSemanticModel( syntaxTree );
            this.Visit( syntaxTree.GetRoot() );
        }

        public override void VisitMemberAccessExpression( MemberAccessExpressionSyntax node )
        {
            this.ValidateSymbol( node.Name, ReferenceKinds.MemberAccess );
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

        private void ValidateSymbol( SyntaxNode? node, ReferenceKinds referenceKind )
        {
            if ( node == null )
            {
                return;
            }

            var symbol = this._semanticModel!.GetSymbolInfo( node ).Symbol;

            this.ValidateSymbol( node, symbol, referenceKind );
        }

        private void ValidateSymbol( SyntaxNode node, ISymbol? symbol, ReferenceKinds referenceKinds )
        {
            if ( symbol == null )
            {
                return;
            }

            var currentDeclaration = this.GetCurrentDeclaration();

            if ( currentDeclaration == null )
            {
                return;
            }

            var validators = this._validatorsBySymbol[symbol];

            foreach ( var validator in validators )
            {
                if ( (validator.ReferenceKinds & referenceKinds) != 0 )
                {
                    validator.Validate( currentDeclaration, node, referenceKinds, this._diagnosticAdder );
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
            private readonly Visitor _parent;

            public ContextCookie( Visitor parent )
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
    }
}