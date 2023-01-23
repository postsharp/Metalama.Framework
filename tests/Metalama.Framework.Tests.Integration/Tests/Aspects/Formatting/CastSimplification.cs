using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

#pragma warning disable CS0169 // Field is not used
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Formatting.CastSimplification;

class Aspect : OverrideMethodAspect
{
    public override dynamic OverrideMethod()
    {
        var clone = meta.Cast(meta.Target.Type, meta.Base.MemberwiseClone());

        foreach (var field in meta.Target.Type.Fields)
        {
            field.Invokers.Final.SetValue(
                clone,
                meta.Cast(field.Type, ((ICloneable)field.Invokers.Final.GetValue(meta.This)).Clone()));
        }

        return clone;
    }
}

class TargetCode : ICloneable
{
    string s;
    TargetCode tc;

    [Aspect]
    TargetCode Method() => throw new NotImplementedException();

    object ICloneable.Clone() => new();
}