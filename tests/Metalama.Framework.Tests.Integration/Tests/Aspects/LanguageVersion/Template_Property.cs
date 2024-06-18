#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
#endif

#if ROSLYN_4_4_0_OR_GREATER

using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.LanguageVersion.Template_Property;

public class TheAspect : TypeAspect
{
    [Introduce]
    public string Property1
    {
        get
        {
            return """get""";
        }
    }

    [Introduce]
    public string Property2
    {
        get => "";
        set
        {
            Console.WriteLine( """set""" );
        }
    }
}

// <target>
[TheAspect]
internal class Target { }

#endif