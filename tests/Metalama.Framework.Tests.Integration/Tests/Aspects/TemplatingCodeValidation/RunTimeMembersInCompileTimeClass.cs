using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplatingCodeValidation.RunTimeMembersInCompileTimeClass;

[CompileTime]
public class CompileTimeClass
{
    // The following members are implicitly CompileTime because they are unmarked and they are contained in a compile-time type.
    private RunTimeClass _runTimeField;

    private RunTimeClass RunTimeProperty { get; set; }

    private void RunTimeMethod( RunTimeClass c ) { }

    private event Action<RunTimeClass> RunTimeEvent;
}

public class RunTimeClass { }