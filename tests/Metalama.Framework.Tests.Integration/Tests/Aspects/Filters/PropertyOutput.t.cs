using System;
using Metalama.Framework.Aspects;

#pragma warning disable CS8618

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Filters.PropertyOutput
{
#pragma warning disable CS0067
    internal class NotNullAttribute : FilterAspect
    {
        public override void Filter(dynamic? value) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

    }
#pragma warning restore CS0067

    internal class Target
    {
        private string q;

        [NotNull]
        public string P 
{ get
{
        global::System.String returnValue ;returnValue = "p";
goto __aspect_return_1;
__aspect_return_1:        if (returnValue == null)
        {
            throw new global::System.ArgumentNullException();
        }

        return returnValue;

}}

        [NotNull]
        public string Q
        {
            get
            {
        global::System.String returnValue ;                returnValue = q;
goto __aspect_return_1;
__aspect_return_1:        if (returnValue == null)
        {
            throw new global::System.ArgumentNullException();
        }

        return returnValue;
            }
        }
        
        
    }
}
