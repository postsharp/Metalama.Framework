using System;
using Newtonsoft.Json;
using Caravela.Patterns.Costura;
using Soothsilver.Random;
using Xunit;

[assembly: CosturaAspect]

namespace Caravela.Patterns.Costura.TestApp
{
    class Program
    {
        static void Main()
        {
            Delay();
        }

        private static void Delay()
        {
            ThenUse.Stuff();
        }
    }

    class ThenUse
    {
        public static void Stuff()
        {
            string srls = JsonConvert.SerializeObject(new string[] { "he", "ha" });
            string r = srls + R.Next(0, 1).ToString();
            Assert.Equal(@"[""he"",""ha""]0", r);
            Console.WriteLine("This is still working: " + r);
        }
    }
}