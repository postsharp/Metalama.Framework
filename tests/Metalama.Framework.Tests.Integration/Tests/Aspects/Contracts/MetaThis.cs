using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.MetaThis;

public class ActionSpeed : ContractAspect
{
    public override void Validate(dynamic? value)
    {
        if (meta.This is IBuffable)
        {
            //apply action speed modifiers
            value = value * 2;
        }
    }
}

interface IBuffable
{
}

// <target>
class Target
{
    int MaybeBuff([ActionSpeed] int speed) => speed;
}