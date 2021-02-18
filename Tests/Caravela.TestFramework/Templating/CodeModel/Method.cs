using System;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using MethodKind = Caravela.Framework.Code.MethodKind;
using SourceCompilation = Caravela.Framework.Impl.CodeModel.SourceCompilation;

namespace Caravela.TestFramework.Templating.CodeModel
{
    internal class Method : IMethod
    {
        private readonly IMethodSymbol _symbol;
        private readonly SourceCompilation _compilation;

        public Method( IMethodSymbol symbol, SourceCompilation compilation )
        {
            this._symbol = symbol;
            this._compilation = compilation;
        }

        public IParameter? ReturnParameter => throw new NotImplementedException();

        public IType ReturnType => new NamedType( (INamedTypeSymbol) this._symbol.ReturnType, this._compilation);

        public IImmutableList<IMethod> LocalFunctions => throw new NotImplementedException();

        public IImmutableList<IParameter> Parameters => this._symbol.Parameters.Select( p => new Parameter( p, this._compilation ) ).ToImmutableList<IParameter>();

        public IImmutableList<IGenericParameter> GenericParameters => throw new NotImplementedException();

        public IImmutableList<IType> GenericArguments => throw new NotImplementedException();

        public bool IsOpenGeneric => throw new NotImplementedException();

        public MethodKind MethodKind => throw new NotImplementedException();

        public bool HasBase => throw new NotImplementedException();

        public IMethodInvocation Base => throw new NotImplementedException();

        public string Name => this._symbol.Name;

        public bool IsStatic => throw new NotImplementedException();

        public bool IsVirtual => throw new NotImplementedException();

        public INamedType? DeclaringType => this._symbol.ContainingType == null ? null : new NamedType( this._symbol.ContainingType, this._compilation );

        public ICodeElement? ContainingElement => throw new NotImplementedException();

        public IReactiveCollection<IAttribute> Attributes => throw new NotImplementedException();

        public CodeElementKind ElementKind => CodeElementKind.Method;

        public dynamic Invoke( dynamic instance, params dynamic[] args )
        {
            throw new NotImplementedException();
        }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
        {
            throw new NotImplementedException();
        }

        public IMethod WithGenericArguments( params IType[] genericArguments )
        {
            throw new NotImplementedException();
        }
    }
}
