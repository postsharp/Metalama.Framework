namespace Metalama.Framework.Introspection;

public enum IntrospectionChildKinds
{
    None,
    DerivedType = 1,
    ContainingDeclaration = 2,
    All = DerivedType | ContainingDeclaration
}