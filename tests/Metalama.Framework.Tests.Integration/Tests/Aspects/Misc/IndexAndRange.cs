using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Aspects.Misc.IndexAndRange
{
    public class UseIndexAndRangeAttribute : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            var baseType = meta.Target.Method.DeclaringType.BaseType!;
            var secondToLastBaseTypeTypeArgument = (INamedType)baseType.TypeArguments[^1];
            var numberOfbaseTypeTypeArgumentsExceptTheLastOne = baseType.TypeArguments.ToArray().AsSpan()[..^1].Length;

            Console.WriteLine($"Second to last base type type argument: {secondToLastBaseTypeTypeArgument}");
            Console.WriteLine($"Number of base type type arguments except the last one: {numberOfbaseTypeTypeArgumentsExceptTheLastOne}");

            return meta.Proceed();
        }

        [CompileTime]
        private void GetDataClassProperties(INamedType baseType)
        {
            var secondToLastBaseTypeTypeArgument = (INamedType)baseType.TypeArguments[^1];
            var numberOfbaseTypeTypeArgumentsExceptTheLastOne = baseType.TypeArguments.ToArray().AsSpan()[..^1].Length;
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