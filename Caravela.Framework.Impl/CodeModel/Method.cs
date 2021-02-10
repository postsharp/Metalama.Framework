using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MethodKind = Caravela.Framework.Code.MethodKind;
using RoslynMethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class Method : CodeElement, IMethod, IMethodInternal
    {
        private readonly IMethodSymbol _symbol;

        protected internal override ISymbol Symbol => this._symbol;

        internal override SourceCompilation Compilation { get; }

        public Method( IMethodSymbol symbol, SourceCompilation compilation )
        {
            this._symbol = symbol;
            this.Compilation = compilation;
        }

        [Memo]
        public IParameter? ReturnParameter => ((IMethod) this).MethodKind is MethodKind.Constructor ? null : new MethodReturnParameter( this );

        [Memo]
        public IType ReturnType => this.SymbolMap.GetIType( this._symbol.ReturnType );

        [Memo]
        public IImmutableList<IMethod> LocalFunctions =>
            this._symbol.DeclaringSyntaxReferences
                .Select( r => r.GetSyntax() )

                // don't descend into nested local functions
                .SelectMany( n => n.DescendantNodes( descendIntoChildren: c => c == n || c is not LocalFunctionStatementSyntax ) )
                .OfType<LocalFunctionStatementSyntax>()
                .Select( f => (IMethodSymbol) this.Compilation.RoslynCompilation.GetSemanticModel( f.SyntaxTree ).GetDeclaredSymbol( f )! )
                .Select( s => this.SymbolMap.GetMethod( s ) )
                .ToImmutableList();

        [Memo]
        public IImmutableList<IParameter> Parameters => this._symbol.Parameters.Select( p => new Parameter( p, this ) ).ToImmutableList<IParameter>();

        [Memo]
        public IImmutableList<IGenericParameter> GenericParameters =>
            this._symbol.TypeParameters.Select( tp => this.SymbolMap.GetGenericParameter( tp ) ).ToImmutableList();

        MethodKind IMethod.MethodKind => this._symbol.MethodKind switch
        {
            RoslynMethodKind.Ordinary => MethodKind.Default,
            RoslynMethodKind.Constructor => MethodKind.Constructor,
            RoslynMethodKind.StaticConstructor => MethodKind.StaticConstructor,
            RoslynMethodKind.Destructor => MethodKind.Finalizer,
            RoslynMethodKind.PropertyGet => MethodKind.PropertyGet,
            RoslynMethodKind.PropertySet => MethodKind.PropertySet,
            RoslynMethodKind.EventAdd => MethodKind.EventAdd,
            RoslynMethodKind.EventRemove => MethodKind.EventRemove,
            RoslynMethodKind.EventRaise => MethodKind.EventRaise,
            RoslynMethodKind.ExplicitInterfaceImplementation => MethodKind.ExplicitInterfaceImplementation,
            RoslynMethodKind.Conversion => MethodKind.ConversionOperator,
            RoslynMethodKind.UserDefinedOperator => MethodKind.UserDefinedOperator,
            RoslynMethodKind.LocalFunction => MethodKind.LocalFunction,
            RoslynMethodKind.AnonymousFunction or
            RoslynMethodKind.BuiltinOperator or
            RoslynMethodKind.DelegateInvoke or
            RoslynMethodKind.ReducedExtension or
            RoslynMethodKind.DeclareMethod or
            RoslynMethodKind.FunctionPointerSignature => throw new NotSupportedException(),
            _ => throw new InvalidOperationException()
        };

        public string Name => this._symbol.Name;

        public bool IsStatic => this._symbol.IsStatic;

        [Memo]
        public override ICodeElement ContainingElement => this._symbol.ContainingSymbol switch
        {
            INamedTypeSymbol type => this.SymbolMap.GetNamedType( type ),
            IMethodSymbol method => this.SymbolMap.GetMethod( method ),
            _ => throw new InvalidOperationException()
        };

        [Memo]
        public override IReactiveCollection<IAttribute> Attributes => this._symbol.GetAttributes().Select( a => new Attribute( a, this.SymbolMap ) ).ToImmutableReactive();

        public override CodeElementKind ElementKind => CodeElementKind.Method;

        public bool IsVirtual => this._symbol.IsVirtual;

        [Memo]
        public INamedType? DeclaringType => this._symbol.ContainingType == null ? null : this.SymbolMap.GetNamedType( this._symbol.ContainingType );

        IReadOnlyList<ISymbol> IMethodInternal.LookupSymbols()
        {
            if ( this._symbol.DeclaringSyntaxReferences.Length == 0 )
            {
                throw new InvalidOperationException();
            }

            var syntaxReference = this._symbol.DeclaringSyntaxReferences[0];
            var semanticModel = this.Compilation.RoslynCompilation.GetSemanticModel( syntaxReference.SyntaxTree );
            var methodBodyNode = ((BaseMethodDeclarationSyntax) syntaxReference.GetSyntax()).Body;
            var lookupPosition = methodBodyNode != null ? methodBodyNode.Span.Start : syntaxReference.Span.Start;

            return semanticModel.LookupSymbols( lookupPosition );
        }

        public override string ToString() => this._symbol.ToString();

        internal sealed class MethodReturnParameter : ReturnParameter
        {
            public Method Method { get; }

            public MethodReturnParameter( Method method )
            {
                this.Method = method;
            }

            protected override Microsoft.CodeAnalysis.RefKind SymbolRefKind => this.Method._symbol.RefKind;

            public override IType Type => this.Method.ReturnType;

            public override ICodeElement ContainingElement => this.Method;

            [Memo]
            public override IReactiveCollection<IAttribute> Attributes =>
                this.Method._symbol.GetReturnTypeAttributes().Select( a => new Attribute( a, this.Method.SymbolMap ) ).ToImmutableReactive();
        }
    }
}
