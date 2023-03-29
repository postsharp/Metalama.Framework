using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.CodeModel.NullableTypes;

public class OverrideAttribute : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        TypeFactory.GetType(typeof(RT));
        TypeFactory.GetType(typeof(RTOCT));
        
        return default;
    }
}

[RunTimeOrCompileTime]
class RTOCT { }

class RT { }

// <target>
internal class TargetClass
{
   
    [Override]
    public TargetClass? TargetMethod_Void(object o, decimal d)
    {
        return null;
    }
}
