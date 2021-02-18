using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using CompilationModel = Caravela.Framework.Impl.CodeModel.Symbolic.CompilationModel;
using MethodKind = Caravela.Framework.Code.MethodKind;

namespace Caravela.TestFramework.Templating.CodeModel
{
    internal class Method : CodeElement, IMethod
    {
        private readonly IMethodSymbol _symbol;
        private readonly CompilationModel _compilation;

        public Method( IMethodSymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this._symbol = symbol;
            this._compilation = compilation;
        }

        protected internal override ISymbol Symbol => this._symbol;

        public IParameter ReturnParameter => throw new NotImplementedException();

        public IType ReturnType => new NamedType( (INamedTypeSymbol) this._symbol.ReturnType, this._compilation );

        public IReadOnlyList<IMethod> LocalFunctions => throw new NotImplementedException();

        public IReadOnlyList<IParameter> Parameters => this._symbol.Parameters.Select( p => new Parameter( p, this._compilation ) ).ToImmutableList<IParameter>();

        public IReadOnlyList<IGenericParameter> GenericParameters => throw new NotImplementedException();

        public IReadOnlyList<IType> GenericArguments => throw new NotImplementedException();

        public bool IsOpenGeneric => throw new NotImplementedException();

        public MethodKind MethodKind => throw new NotImplementedException();

        public bool HasBase => throw new NotImplementedException();

        public IMethodInvocation Base => throw new NotImplementedException();

        public string Name => this._symbol.Name;

        public bool IsStatic => throw new NotImplementedException();

        public bool IsVirtual => throw new NotImplementedException();

        public INamedType? DeclaringType => this._symbol.ContainingType == null ? null : new NamedType( this._symbol.ContainingType, this._compilation );

        public override CodeElementKind ElementKind => CodeElementKind.Method;

        public Framework.Code.Accessibility Accessibility => throw new NotImplementedException();

        public bool IsAbstract => throw new NotImplementedException();

        public bool IsSealed => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public bool IsOverride => throw new NotImplementedException();

        public bool IsNew => throw new NotImplementedException();

        public bool IsAsync => throw new NotImplementedException();

        public dynamic Invoke( dynamic? instance, params dynamic[] args ) => throw new NotImplementedException();

        public IMethod WithGenericArguments( params IType[] genericArguments ) => throw new NotImplementedException();
    }
}
