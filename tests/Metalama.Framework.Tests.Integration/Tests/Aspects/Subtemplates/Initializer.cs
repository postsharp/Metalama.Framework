using System;
using JetBrains.FormatRipper;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Initializer;

internal class Aspect : TypeAspect
{
    [Introduce]
    int i = Compute();

    [Template]
    private static int Compute()
    {
        Console.WriteLine("called template");
        return 42;
    }
}

// <target>
[Aspect]
class TargetCode
{
}