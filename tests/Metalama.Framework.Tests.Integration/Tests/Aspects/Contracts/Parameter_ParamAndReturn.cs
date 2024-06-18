using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS8618

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Parameter_ParamAndReturn
{
    internal class FilterAttribute : MethodAspect
    {
        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder.Advice.AddContract( builder.Target.ReturnParameter, nameof(Filter) );

            foreach (var parameter in builder.Target.Parameters)
            {
                builder.Advice.AddContract( parameter, nameof(Filter) );
            }
        }

        [Template]
        public void Filter( dynamic? value )
        {
            if (value == null)
            {
                throw new ArgumentNullException( meta.Target.Parameter.Name );
            }
        }
    }

    // <target>
    internal class Target
    {
        [Filter]
        private string? M( string? param1, int? param2 )
        {
            return param1 + param2.ToString();
        }
    }
}