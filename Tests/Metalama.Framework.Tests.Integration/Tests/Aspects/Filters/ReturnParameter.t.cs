using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.ReturnParameter
{
#pragma warning disable CS0067
    internal class NotNullAttribute : FilterAspect
    {
        public override void Filter(dynamic? value) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

    }
#pragma warning restore CS0067

    internal class Target
    {
        [return: NotNull]
        private string M()
        {
    global::System.String returnValue;
            returnValue = "";
goto __aspect_return_1;
__aspect_return_1:    if (returnValue == null)
    {
        throw new global::System.ArgumentNullException("<return>");
    }

    return returnValue;
        }
    }
}