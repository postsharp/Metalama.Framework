using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;
namespace Metalama.Framework.Tests.Integration.Aspects.Misc.IndexAndRange
{
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
    public class UseIndexAndRangeAttribute : OverrideMethodAspect
    {
        public override dynamic OverrideMethod() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
        [CompileTime]
        private void GetDataClassProperties(INamedType baseType) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
    }
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
    class GenericType<T1, T2>
    {
    }
    internal class TargetCode : GenericType<int, int>
    {
        [UseIndexAndRange]
        private int Method(int a, int b, int c, int d)
        {
            global::System.Console.WriteLine("Second to last base type type argument: int");
            global::System.Console.WriteLine("Number of base type type arguments except the last one: 1");
            return a;
        }
    }
}
