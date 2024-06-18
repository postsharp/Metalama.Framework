using Metalama.Framework.Aspects;
using System;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Fields.PrivateField;

public class IntroducePrivateFieldAttribute : OverrideMethodAspect
{
    [Introduce]
    private readonly string _text = "a text";

    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( _text );

        return meta.Proceed();
    }
}