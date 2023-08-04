using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Async.AsyncTemplate.ProceedAsyncAsyncDefaultTemplate;

class Aspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.Override(builder.Target, nameof(Template));
    }

    [Template]
    async Task Template()
    {
        await meta.ProceedAsync();
    }
}

// <target>
class TargetCode
{
    [Aspect]
    async Task AsyncTaskMethod()
    {
        await Task.Yield();
    }
}