﻿using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Methods.PartialType_SyntaxTrees
{
    // <target>
    internal partial class TargetClass
    {
        public void TargetMethod3()
        {
            Console.WriteLine("This is TargetMethod3.");
        }
    }
}