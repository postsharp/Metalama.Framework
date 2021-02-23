using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Caravela.Framework.Impl.CodeModel
{
    internal partial class Method : MethodBase, IMethodInternal
    {

        public Method( IMethodSymbol symbol, CompilationModel compilation ) : base( symbol, compilation )
        {
        }

        [Memo]
        public IParameter ReturnParameter => new MethodReturnParameter( this );

        [Memo]
        public IType ReturnType => this.Compilation.Factory.GetIType( this.MethodSymbol.ReturnType );

        [Memo]
        public IGenericParameterList GenericParameters =>
            new GenericParameterList(
                this.MethodSymbol.TypeParameters.Select( tp => CodeElementLink.FromSymbol<IGenericParameter>( tp ) ),
                this.Compilation );

        public override CodeElementKind ElementKind => CodeElementKind.Method;

        [Memo]
        public IReadOnlyList<IType> GenericArguments =>
            this.MethodSymbol.TypeArguments.Select( this.Compilation.Factory.GetIType ).ToImmutableList();

        public bool IsOpenGeneric => this.GenericArguments.Any( ga => ga is IGenericParameter ) || this.DeclaringType?.IsOpenGeneric == true;

        public object Invoke( object? instance, params object[] args ) => new MethodInvocation( this ).Invoke( instance, args );

        public bool HasBase => true;

        public IMethodInvocation Base => new MethodInvocation( this ).Base;

        public IMethod WithGenericArguments( params IType[] genericArguments )
        {
            var symbolWithGenericArguments = this.MethodSymbol.Construct( genericArguments.Select( a => a.GetSymbol() ).ToArray() );

            return new Method( symbolWithGenericArguments, this.Compilation );
        }

        IReadOnlyList<ISymbol> IMethodInternal.LookupSymbols()
        {
            if ( this.Symbol.DeclaringSyntaxReferences.Length == 0 )
            {
                throw new InvalidOperationException();
            }

            var syntaxReference = this.Symbol.DeclaringSyntaxReferences[0];
            var semanticModel = this.Compilation.RoslynCompilation.GetSemanticModel( syntaxReference.SyntaxTree );
            var methodBodyNode = ((MethodDeclarationSyntax) syntaxReference.GetSyntax()).Body;
            var lookupPosition = methodBodyNode != null ? methodBodyNode.Span.Start : syntaxReference.Span.Start;

            return semanticModel.LookupSymbols( lookupPosition );
        }

        public override bool IsReadOnly => this.MethodSymbol.IsReadOnly;

        public override bool IsAsync => this.MethodSymbol.IsAsync;
    }
}
