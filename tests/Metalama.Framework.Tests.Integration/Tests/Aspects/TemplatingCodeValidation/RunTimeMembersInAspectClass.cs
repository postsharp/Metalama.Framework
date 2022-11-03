using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplatingCodeValidation.RunTimeMembersInAspectClass;

public class MyAspect : TypeAspect
{
    // All these declarations must be forbidden because run-time members are allowed only in templates.

    private RunTimeClass _runTimeField;

    private RunTimeClass RunTimeProperty { get; set; }

    private void RunTimeMethod( RunTimeClass c ) { }

    private event Action<RunTimeClass> RunTimeEvent;
}

public class RunTimeClass { }