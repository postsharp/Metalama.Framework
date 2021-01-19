using System.Collections.Immutable;

namespace Caravela.Framework.Code
{
    public interface IGenericParameter : ICodeElement, IType
    {
        string Name { get; }
        int Index { get; }
        IImmutableList<IType> TypeConstraints { get; }
        bool IsCovariant { get; }
        bool IsContravariant { get; }
        bool HasDefaultConstructorConstraint { get; }
        bool HasReferenceTypeConstraint { get; }
        bool HasNonNullableValueTypeConstraint { get; }
        // TODO: nullable reference type constraints
    }
}