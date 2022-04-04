using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.EventFields.PartialType_SyntaxTrees
{
    // <target>
    internal partial class TargetClass
    {
        public event EventHandler? TargetEvent3;
    }
}