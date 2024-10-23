﻿using Metalama.Framework.Aspects;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.LocalVariableNameCollision;

#pragma warning disable CS0168, CS0164

class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        int a, b, c, d, e, f, g, h, i, j, k, l, m, n;
        return meta.Proceed();
    }
}

// <target>
class TargetClass
{
    [Aspect]
    public int TargetMethod()
    {
        var a = 0;

        var (b, _) = (1, 2);

        (var c, var _, _) = (3, 4, 5);

        int.TryParse("1", out var d);
        int.TryParse("2", out var _);
        int.TryParse("3", out _);

        _ = from e in new int[0]
            let f = e
            join g in new int[0] on e equals g
            join h in new int[0] on f equals h into i
            group e by e into j
            select j into k
            select k;

    l:;

        foreach (var m in new int[] { })
        {
        }

        try
        {
        }
        catch (Exception)
        {
        }

        try
        {
        }
        catch (Exception n)
        {
        }

        return 0;
    }
}