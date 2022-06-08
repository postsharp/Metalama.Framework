using Issue30557ClassLibrary;
using System;

namespace Issue30557Program
{
    class Program
    {
        static int Main()
        {
            var x = new TargetClass();

            Console.WriteLine( x.A );

            return ( x.A == "A" ) ? 0 : 1;
        }
    }

    [StringPropertyAspect]
    internal class TargetClass
    {
        public string A { get; set; } = "a";
    }
}