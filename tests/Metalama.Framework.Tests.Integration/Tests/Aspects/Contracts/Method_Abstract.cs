using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Method_Abstract;

internal class NotNullAttribute : ContractAspect
{
    public override void Validate( dynamic? value )
    {
        if (value == null)
        {
            throw new ArgumentNullException( meta.Target.Parameter.Name );
        }
    }
}

// <target>
abstract class Base
{
    public abstract void M([NotNull] string m);
}

// <target>
class Target : Base
{
    public override void M(string m)
    {
    }
}