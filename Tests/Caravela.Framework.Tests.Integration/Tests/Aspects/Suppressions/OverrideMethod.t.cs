// Warning CS0219 on `x`: `The variable 'x' is assigned but its value is never used`
internal class TargetClass
    {
#pragma warning disable CS0219
        [SuppressWarning]
        private void M2( string m ) 
{
    int a = 0;
            int x = 0;
    return;
}
#pragma warning restore CS0219
            
        // CS0219 expected 
        private void M1( string m ) 
        {
            int x = 0;
        }
    }