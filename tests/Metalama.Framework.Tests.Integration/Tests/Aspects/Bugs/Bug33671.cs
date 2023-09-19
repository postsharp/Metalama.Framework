using System;
using System.IO;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33671;

public class TestAspect : TypeAspect, IFace
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
    }

    public string? ProfileName { get; init; }
}

// <target>
[TestAspect]
class Target
{
}