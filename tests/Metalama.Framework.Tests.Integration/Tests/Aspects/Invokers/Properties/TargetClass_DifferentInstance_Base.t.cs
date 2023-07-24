public class TargetClass
{
    public int Property
    {
        get
        {
            return 0;
        }
        set
        {
        }
    }
    private TargetClass? instance;
    [InvokerAspect]
    public int Invoker
    {
        get
        {
            _ = ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.TargetClass_DifferentInstance_Base.TargetClass)this.instance!).Property;
            return 0;
        }
        set
        {
            ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.TargetClass_DifferentInstance_Base.TargetClass)this.instance!).Property = 42;
        }
    }
}
