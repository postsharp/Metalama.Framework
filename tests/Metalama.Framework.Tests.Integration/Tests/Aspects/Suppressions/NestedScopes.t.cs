// Warning CS0219 on `a`: `The variable 'a' is assigned but its value is never used`
// Warning CS0219 on `y`: `The variable 'y' is assigned but its value is never used`
internal class TargetClass
{
    [SuppressWarning]
    private void M2(string m)
    {
        var a = 0;
#pragma warning disable CS0219
        var x = 0;
#pragma warning restore CS0219
        var y = 0;
        return;
    }
    private void M1(string m)
    {
#pragma warning disable CS0219
        var x = 0;
#pragma warning restore CS0219
        // CS0219 expected
        var y = 0;
    }
}
