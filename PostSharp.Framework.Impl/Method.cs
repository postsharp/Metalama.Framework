using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PostSharp.Framework.Impl
{
    internal class Method : CodeElement, IMethod
    {
        private readonly IMethodSymbol symbol;
        internal override Compilation Compilation { get; }

        public Method(IMethodSymbol symbol, Compilation compilation)
        {
            this.symbol = symbol;
            Compilation = compilation;
        }

        [LazyThreadSafeProperty]
        public IParameter ReturnParameter => new ReturnParameterImpl(this);

        [LazyThreadSafeProperty]
        public IType ReturnType => Cache.GetIType(symbol.ReturnType);

        [LazyThreadSafeProperty]
        public IReadOnlyList<IMethod> LocalFunctions =>
            symbol.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                // don't descend into nested local functions
                .SelectMany(n => n.DescendantNodes(descendIntoChildren: c => c == n || c is not LocalFunctionStatementSyntax))
                .OfType<LocalFunctionStatementSyntax>()
                .Select(f => (IMethodSymbol)Compilation.RoslynCompilation.GetSemanticModel(f.SyntaxTree).GetDeclaredSymbol(f))
                .Select(s => new Method(s, Compilation))
                .ToImmutableArray();

        [LazyThreadSafeProperty]
        public IReadOnlyList<IParameter> Parameters => symbol.Parameters.Select(p => new Parameter(p, this)).ToImmutableArray();

        public IReadOnlyList<IGenericParameter> GenericParameters => throw new NotImplementedException();

        public string Name => symbol.Name;

        public bool IsStatic => symbol.IsStatic;

        public override ICodeElement ContainingElement => throw new NotImplementedException();

        public override IReadOnlyList<IAttribute> Attributes => throw new NotImplementedException();

        public override string ToString() => symbol.ToString();

        private class ReturnParameterImpl : IParameter
        {
            private readonly Method method;

            public ReturnParameterImpl(Method method) => this.method = method;

            public IType Type => method.ReturnType;

            public string? Name => null;

            public int Index => -1;

            public ICodeElement? ContainingElement => method;

            [LazyThreadSafeProperty]
            public IReadOnlyList<IAttribute> Attributes => method.symbol.GetReturnTypeAttributes().Select(a => new Attribute(a, method.Cache)).ToImmutableArray();
        }
    }
}
