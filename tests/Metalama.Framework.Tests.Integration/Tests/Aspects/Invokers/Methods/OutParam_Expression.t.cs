public class C
{
    private object _field = new();

    [TheAspect]
    public void Method( out object o )
    {
        Method_Source( out _field );
        Method_Source( out o );

        return;
    }

    private void Method_Source( out object o )
    {
        o = new object();
    }
}