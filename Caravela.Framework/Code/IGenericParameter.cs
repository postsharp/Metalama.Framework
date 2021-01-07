using System.Collections.Immutable;

namespace Caravela.Framework.Code
{
    public interface IGenericParameter : ICodeElement
    {
        string Name { get; }
        int Index { get; }
        IImmutableList<IType> BaseTypeConstraints { get; }
        bool IsCovariant { get; }
        bool IsContravariant { get; }
        bool HasDefaultConstructorConstraint { get; }
        bool HasReferenceTypeConstraint { get; }
        bool HasNotNullableValueTypeConstraint { get; }
        // TODO: nullable reference type constraints
    }
}