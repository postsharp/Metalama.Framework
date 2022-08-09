﻿using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Operators.PartialType_SyntaxTrees
{
    // <target>
    internal partial class TargetClass
    {
        public static TargetClass operator *(TargetClass a, TargetClass b)
        {
            Console.WriteLine($"This is the original operator.");

            return new TargetClass();
        }
    }
}