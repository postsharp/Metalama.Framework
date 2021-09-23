using System;
using System.ComponentModel;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Tests.Aspects.Introductions.Methods.Bug28969
{
    class MyAspect : Attribute, IAspect<INamedType>
    {
        public void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            var methodBuilder = builder.Advices.IntroduceMethod(
                builder.Target, 
                nameof(Method));

        }


        [Template(IsVirtual = true)]
        public void Method() {}
    }
    
    // <target>
    [MyAspect]
    class TargetCode
    {
    }
}