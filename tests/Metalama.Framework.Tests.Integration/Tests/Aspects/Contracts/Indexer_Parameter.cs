using System;
using Metalama.Framework.Aspects;

#pragma warning disable CS8618, CS0169, CS0649

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Indexer_Parameter
{
    internal class NotZeroAttribute : ContractAspect
    {
        public override void Validate(dynamic? value)
        {
            if (value == 0)
            {
                throw new ArgumentException();
            }
        }
    }

    // <target>
    internal class Target
    {
        private int q;
        
        public int this[[NotZero] int x]
        {
            get
            {
                System.Console.WriteLine("Original body");
                return 42;
            }

            set
            {
                System.Console.WriteLine("Original body");
            }
        }

        public int this[[NotZero] int x, [NotZero] int y]
        {
            get
            {
                System.Console.WriteLine("Original body");
                return 42;
            }
            set
            {
                System.Console.WriteLine("Original body");
            }
        }
    }
}