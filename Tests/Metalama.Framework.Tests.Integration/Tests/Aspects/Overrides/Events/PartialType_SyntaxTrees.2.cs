﻿using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Events.PartialType_SyntaxTrees
{
    // <target>
    internal partial class TargetClass
    {
        public event EventHandler TargetEvent3
        {
            add => Console.WriteLine("This is TargetEvent3.");
            remove => Console.WriteLine("This is TargetEvent3.");
        }
    }
}