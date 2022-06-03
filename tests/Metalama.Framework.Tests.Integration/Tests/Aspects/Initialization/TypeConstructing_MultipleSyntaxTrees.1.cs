using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.TypeConstructing_MultipleSyntaxTrees
{
    // <target>
    public partial class TargetCode
    {
        static TargetCode()
        {
        }

        public void Bar()
        {
        }
    }
}