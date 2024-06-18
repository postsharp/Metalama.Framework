#if ROSLYN_4_4_0_OR_GREATER

using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.LanguageVersion.Template_Event_OldVersion;

public class TheAspect : TypeAspect
{
    [Introduce]
    public event EventHandler Event1
    {
        add
        {
            Console.WriteLine( """add""" );
        }
        remove { }
    }

    [Introduce]
    public event EventHandler Event2
    {
        add { }
        remove
        {
            Console.WriteLine( """remove""" );
        }
    }
}

#endif