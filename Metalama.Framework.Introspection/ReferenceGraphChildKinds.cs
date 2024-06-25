namespace Metalama.Framework.Introspection;

public enum ReferenceGraphChildKinds
{
    None,
    DerivedType = 1,
    ContainingDeclaration = 2,
    All = DerivedType | ContainingDeclaration
}