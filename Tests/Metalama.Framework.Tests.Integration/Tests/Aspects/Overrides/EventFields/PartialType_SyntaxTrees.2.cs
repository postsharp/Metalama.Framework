using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.EventFields.PartialType_SyntaxTrees
{
    // <target>
    internal partial class TargetClass
    {
        public event EventHandler? TargetEvent3;
    }
}