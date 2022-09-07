[Introduction]
[Override]
internal class TargetClass : BaseClass
{
    public void TargetClass_Method()
    {
        // Introduced.
        global::System.Console.WriteLine("Override");
        this?.TargetClass_Method_Source();
        goto __aspect_return_1;

    __aspect_return_1:;
    }

    private void TargetClass_Method_Source()
    {
        System.Console.WriteLine("TargetClass_Method()");
    }


    public new void BaseClass_Method()
    {
        // Introduced.
        base.BaseClass_Method();
    }
}