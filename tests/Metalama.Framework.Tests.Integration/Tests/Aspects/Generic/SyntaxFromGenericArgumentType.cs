using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Generic.SyntaxFromGenericArgumentType;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic OverrideMethod()
    {
        Console.WriteLine(meta.Target.Method.ReturnType.ToType());

        return meta.Proceed();
    }
}

// <target>
internal class TargetCode<T>
{
    [Aspect]
    Task<T> GenericClassTypeParameter() => null!;

    [Aspect]
    Task<TM> GenericMethodTypeParameter<TM>() => null!;

    [Aspect]
    Task<int> ClosedGeneric() => null!;

    [Aspect]
    T[] ArrayClassTypeParameter() => null!;

    [Aspect]
    TM[] ArrayMethodTypeParameter<TM>() => null!;

    [Aspect]
    int[] ClosedArray() => null!;

    [Aspect]
    Action<T, TM, T[], TM[], int, int[], Action<T, TM>> ComplexType<TM>() => null!;
}