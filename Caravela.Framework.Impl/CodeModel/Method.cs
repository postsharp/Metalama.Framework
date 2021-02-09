using System.Collections.Generic;
using System.Collections.Immutable;
using Caravela.Framework.Code;
using MethodKind = Caravela.Framework.Code.MethodKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal abstract class Method : Member, IMethod
    {
        IParameter? IMethod.ReturnParameter => this.ReturnParameter;

        public abstract Parameter? ReturnParameter { get; }

        IType IMethod.ReturnType => this.ReturnType;

        public abstract ITypeInternal ReturnType { get; }

        IReadOnlyList<IMethod> IMethod.LocalFunctions => this.LocalFunctions;

        public abstract IReadOnlyList<Method> LocalFunctions { get; }

        IReadOnlyList<IParameter> IMethod.Parameters => this.Parameters;

        public abstract IReadOnlyList<Parameter> Parameters { get; }

        IReadOnlyList<IGenericParameter> IMethod.GenericParameters => this.GenericParameters;

        public abstract IReadOnlyList<GenericParameter> GenericParameters { get; }

        public abstract MethodKind MethodKind { get; }

        public override CodeElementKind ElementKind => CodeElementKind.Method;
    }
}
