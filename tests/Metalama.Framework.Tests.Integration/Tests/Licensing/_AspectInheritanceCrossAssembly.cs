using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Licensing.AspectInheritanceCrossAssembly.Dependency;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.AspectInheritanceCrossAssembly;

internal class ImplementingClass : IInterfaceWithAspects
{
    public void TargetMethod()
    {
    }
}