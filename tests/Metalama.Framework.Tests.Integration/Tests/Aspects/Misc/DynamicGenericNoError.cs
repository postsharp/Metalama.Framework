using System;
using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

#pragma warning disable CS0169, CS8618

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.DynamicGenericNoError;

// Test that dynamic type constructions are allowed in run-time code.

// <target>
class TargetCode
{
    Action<string, dynamic> dynamicGeneric;
    dynamic[] dynamicArray;
    (dynamic, int) dynamicTuple;
    ref dynamic DynamicRef => throw new Exception();

    Action<string, Func<dynamic, object>> dynamicConstructionGeneric;
    Func<dynamic, object>[] dynamicConstructionArray;
    (Func<dynamic, object>, int) dynamicConstructionTuple;
    ref Func<dynamic, object> DynamicConstructionRef => throw new Exception();
}