// Warning CS0219 on `a`: `The variable 'a' is assigned but its value is never used`
internal class TargetClass
    {
#pragma warning disable CS0219
        [SuppressWarning]
        private void M2( string m )
        {
    var a = 0;
                var x = 0;


            var y = 0;
    return;
        }
#pragma warning restore CS0219

        private void M1( string m )
        {
#pragma warning disable CS0219
            var x = 0;
#pragma warning restore CS0219


            // CS0219 expected 
            var y = 0;
        }
    }