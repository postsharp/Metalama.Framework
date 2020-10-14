using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RoslynMethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Caravela.Framework.Impl
{
    internal class Method : CodeElement, IMethod
    {
        private readonly IMethodSymbol symbol;
        protected override ISymbol Symbol => symbol;

        internal override Compilation Compilation { get; }

        public Method(IMethodSymbol symbol, Compilation compilation)
        {
            this.symbol = symbol;
            Compilation = compilation;
        }

        [Memo]
        public IParameter ReturnParameter => new ReturnParameterImpl(this);

        [Memo]
        public IType ReturnType => Cache.GetIType(symbol.ReturnType);

        [Memo]
        public IReadOnlyList<IMethod> LocalFunctions =>
            symbol.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                // don't descend into nested local functions
                .SelectMany(n => n.DescendantNodes(descendIntoChildren: c => c == n || c is not LocalFunctionStatementSyntax))
                .OfType<LocalFunctionStatementSyntax>()
                .Select(f => (IMethodSymbol)Compilation.RoslynCompilation.GetSemanticModel(f.SyntaxTree).GetDeclaredSymbol(f))
                .Select(s => new Method(s, Compilation))
                .ToImmutableArray();

        [Memo]
        public IReadOnlyList<IParameter> Parameters => symbol.Parameters.Select(p => new Parameter(p, this)).ToImmutableArray();

        public IReadOnlyList<IGenericParameter> GenericParameters => throw new NotImplementedException();

        public MethodKind Kind => symbol.MethodKind switch
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

        public string Name => symbol.Name;

        public bool IsStatic => symbol.IsStatic;

        [Memo]
        public override ICodeElement ContainingElement => symbol.ContainingSymbol switch
        {
            INamedTypeSymbol type => Cache.GetTypeInfo(type),
            IMethodSymbol method => Cache.GetMethod(method),
            _ => throw new InvalidOperationException()
        };

        [Memo]
        public override IReadOnlyList<IAttribute> Attributes => symbol.GetAttributes().Select(a => new Attribute(a, Cache)).ToImmutableArray();

        public override string ToString() => symbol.ToString();

        private class ReturnParameterImpl : IParameter
        {
            private readonly Method method;

            public ReturnParameterImpl(Method method) => this.method = method;

            public IType Type => method.ReturnType;

            public string? Name => null;

            public int Index => -1;

            public ICodeElement? ContainingElement => method;

            [Memo]
            public IReadOnlyList<IAttribute> Attributes => method.symbol.GetReturnTypeAttributes().Select(a => new Attribute(a, method.Cache)).ToImmutableArray();
        }
    }
}
