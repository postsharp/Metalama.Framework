using System;
using System.Linq;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.PartialMethodSymbolPart;

// <target>
internal partial class TargetCode
{
    partial void M();

    partial void M()
    {
    }
}