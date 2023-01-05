using Metalama.Framework.Aspects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.CrossAssembly
{
    // <target>
    [Override]
    [Introduction]
    internal class TargetClass
    {
        public string this[string x]
        {
            get
            {
                Console.WriteLine("Original");
                return x;
            }

            set
            {
                Console.WriteLine("Original");
                x = value;
            }
        }
    }
}