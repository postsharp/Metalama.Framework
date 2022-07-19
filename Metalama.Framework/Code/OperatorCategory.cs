using Metalama.Framework.Aspects;

namespace Metalama.Framework.Code;

[CompileTime]
public enum OperatorCategory
{
    None,
    Unary,
    Binary,
    Conversion
}