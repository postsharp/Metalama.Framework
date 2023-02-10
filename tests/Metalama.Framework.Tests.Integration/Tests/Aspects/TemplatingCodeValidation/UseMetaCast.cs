namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplatingCodeValidation.UseMetaCast;

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;


internal class TestAttribute : TypeAspect
{
    [Template]
    public dynamic? MyTemplate()
    {
        var x = meta.Proceed();

        return meta.Cast(meta.Target.Type, x);
    }
}