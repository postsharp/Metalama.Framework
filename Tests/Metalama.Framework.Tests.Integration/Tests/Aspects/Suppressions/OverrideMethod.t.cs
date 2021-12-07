internal class TargetClass
    {
#pragma warning disable CS0219
        [SuppressWarning]
        private void M2( string m )
{
    var a = 0;
            var x = 0;
    return;
}
#pragma warning restore CS0219

        // CS0219 expected 
        private void M1( string m )
        {
            var x = 0;
        }
    }