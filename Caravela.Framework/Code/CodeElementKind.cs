using Caravela.Framework.Project;

namespace Caravela.Framework.Code
{
    [CompileTime]
    public enum CodeElementKind
    {
        Compilation,
        Type,
        Method,
        Property,
        Field,
        Event,
        Parameter,
        GenericParameter
    }
}