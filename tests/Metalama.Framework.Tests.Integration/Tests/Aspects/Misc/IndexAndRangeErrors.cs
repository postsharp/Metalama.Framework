#if TEST_OPTIONS
// @RequiredConstant(NET5_0_OR_GREATER) - Index and Range are not included in .Net Framework
#endif

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Metalama.Framework.Tests.Integration.Aspects.Misc.IndexAndRangeErrors
{
    public class UseIndexAndRangeAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            var collection = meta.Target.Method.DeclaringType.BaseType!.TypeArguments.Select(ta => ta.ToDisplayString()).ToArray();
            var compileTimeCollection = collection.AsSpan();

            var compileTimeCollectionWithRunTimeIndex = compileTimeCollection[meta.RunTime(^1)];
            Console.WriteLine(compileTimeCollectionWithRunTimeIndex);
            var compileTimeCollectionWithRunTimeRange = compileTimeCollection[meta.RunTime(..^1)].Length;
            Console.WriteLine(compileTimeCollectionWithRunTimeRange);

            return meta.Proceed();
        }
    }

    class GenericType<T1, T2> { }

    internal class TargetCode : GenericType<int, int>
    {
        [UseIndexAndRange]
        private int Method(int a, int b, int c, int d)
        {
            return a;
        }
    }
}