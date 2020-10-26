using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynMethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Caravela.Framework.Impl
{
    internal class Method : CodeElement, IMethod
    {
        private readonly IMethodSymbol _symbol;
        protected internal override ISymbol Symbol => this._symbol;

        internal override SourceCompilation Compilation { get; }

        public Method(IMethodSymbol symbol, SourceCompilation compilation)
        {
            this._symbol = symbol;
            this.Compilation = compilation;
        }

        [Memo]
        public IParameter ReturnParameter => new ReturnParameterImpl(this);

        [Memo]
        public IType ReturnType => this.SymbolMap.GetIType( this._symbol.ReturnType);

        [Memo]
        public IReadOnlyList<IMethod> LocalFunctions =>
            this._symbol.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                // don't descend into nested local functions
                .SelectMany(n => n.DescendantNodes(descendIntoChildren: c => c == n || c is not LocalFunctionStatementSyntax))
                .OfType<LocalFunctionStatementSyntax>()
                .Select(f => (IMethodSymbol) this.Compilation.RoslynCompilation.GetSemanticModel(f.SyntaxTree).GetDeclaredSymbol(f))
                .Select(s => this.SymbolMap.GetMethod(s))
                .ToImmutableArray();

        [Memo]
        public IReadOnlyList<IParameter> Parameters => this._symbol.Parameters.Select(p => new Parameter(p, this)).ToImmutableArray();

        public IReadOnlyList<IGenericParameter> GenericParameters => throw new NotImplementedException();

        public MethodKind Kind => this._symbol.MethodKind switch
        {
            RoslynMethodKind.Ordinary => MethodKind.Ordinary,
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
            INamedTypeSymbol type => this.SymbolMap.GetNamedType(type),
            IMethodSymbol method => this.SymbolMap.GetMethod(method),
            _ => throw new InvalidOperationException()
        };

        [Memo]
        public override IReactiveCollection<IAttribute> Attributes => this._symbol.GetAttributes().Select(a => new Attribute(a, this.SymbolMap )).ToImmutableReactive();

        public bool IsVirtual => this._symbol.IsVirtual;

        [Memo]
        public INamedType? DeclaringType => this._symbol.ContainingType == null ? null : this.SymbolMap.GetNamedType( this._symbol.ContainingType );

        public override string ToString() => this._symbol.ToString();

        private class ReturnParameterImpl : IParameter
        {
            private readonly Method _method;

            public ReturnParameterImpl(Method method) => this._method = method;

            public IType Type => this._method.ReturnType;

            public string? Name => null;

            public int Index => -1;

            public ICodeElement? ContainingElement => this._method;

            [Memo]
            public IReactiveCollection<IAttribute> Attributes =>
                this._method._symbol.GetReturnTypeAttributes().Select(a => new Attribute(a, this._method.SymbolMap)).ToImmutableReactive();
        }
    }
}
