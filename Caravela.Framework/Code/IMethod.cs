using System.Collections.Immutable;

namespace Caravela.Framework.Code
{
    public interface IMethod : IMember
    {
        IParameter ReturnParameter { get; }
        // for convenience
        IType ReturnType { get; }
        IImmutableList<IMethod> LocalFunctions { get; }
        IImmutableList<IParameter> Parameters { get; }
        IImmutableList<IGenericParameter> GenericParameters { get; }
        new MethodKind Kind { get; }

        //dynamic Invoke(params object[] args);
    }
}