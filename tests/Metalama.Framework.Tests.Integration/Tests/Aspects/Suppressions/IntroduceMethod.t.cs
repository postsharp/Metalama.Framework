// Warning CS0219 on `x`: `The variable 'x' is assigned but its value is never used`
// Warning CS0219 on `x`: `The variable 'x' is assigned but its value is never used`
[SuppressWarning]
internal class TargetClass
{
    // CS0219 expected
    private void M1(string m)
    {
        var x = 0;
    }
    public void Introduced()
    {
        var x = 0;
    }
}
