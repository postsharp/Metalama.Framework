// Warning CS0219 on `x`: `The variable 'x' is assigned but its value is never used`

[TestOutput]
internal class TargetClass
{
#pragma warning disable CS0219
    [SuppressWarning]
    private void M2(string m)
    {
        this.__Override__M2__By__Caravela_Framework_Tests_Integration_Aspects_Suppressions_IntroduceMethod_SuppressWarningAttribute(m);
    }
#pragma warning restore CS0219
#pragma warning disable CS0219


    private void __Override__M2__By__Caravela_Framework_Tests_Integration_Aspects_Suppressions_IntroduceMethod_SuppressWarningAttribute(global::System.String m)
    {
        int a = 0;
        int x = 0;
        return;
    }
#pragma warning restore CS0219

    // CS0219 expected 
    private void M1(string m)
    {
        int x = 0;
    }
}