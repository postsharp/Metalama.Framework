using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using MyDateTime = System.DateTime;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.NameOf
{
    internal class MyAspect : ContractAspect
    {
        public override void BuildAspect( IAspectBuilder<IParameter> builder )
        {
            builder.AspectState = new State() { Names = string.Join( ", ", nameof(MyAspect), nameof(MyDateTime), nameof(C), nameof(MyDateTime.UtcNow) ) };
            base.BuildAspect( builder );
        }

        public override void Validate( dynamic? value )
        {
            Console.WriteLine(
                "FromTemplate: " + meta.CompileTime(
                    string.Join( ", ", nameof(value), nameof(MyAspect), nameof(MyDateTime), nameof(C), nameof(MyDateTime.UtcNow) ) ) );

            Console.WriteLine( "FromBuildAspect:" + ( (State) meta.AspectInstance.AspectState! ).Names );
        }

        private class State : IAspectState
        {
            public string Names = "";
        }
    }

    // <target>
    internal class C
    {
        [return: MyAspect]
        public int M( [MyAspect] int p ) => 0;
    }
}