namespace Caravela.Framework.Code
{
    public enum TypeKindConstraint
    {
        None,
        Class,

        // TODO: Must be handled differently, as in Roslyn.
        NullableClass,
        Struct,
        Unmanaged,
        NotNull,
        Default
    }
}