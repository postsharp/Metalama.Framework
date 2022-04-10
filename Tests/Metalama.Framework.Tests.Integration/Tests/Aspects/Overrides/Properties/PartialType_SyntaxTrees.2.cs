﻿using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Properties.PartialType_SyntaxTrees
{
    // <target>
    internal partial class TargetClass
    {
        public int TargetProperty3
        {
            get
            {
                Console.WriteLine("This is TargetProperty3.");
                return 42;
            }

            set => Console.WriteLine("This is TargetProperty3.");
        }
    }
}