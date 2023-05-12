
using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33145;


public sealed class NotNullAttribute : ContractAspect
{
    public override void Validate(dynamic? value)
    {
        if (value == null)
            throw new ArgumentNullException();
    }
}

// <target>
public class Class1
{
    public ValueTask ExecuteAsync([NotNull] Action action) => new(Task.CompletedTask);
}
