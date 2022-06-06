using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.Programmatic
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            {
                var introduced = builder.Advice.IntroduceMethod( builder.Target, nameof(Template) );
                introduced.Name = "IntroducedMethod_Parameters";
                introduced.AddParameter( "x", typeof(int) );
                introduced.AddParameter( "y", typeof(int) );
            }

            {
                var introduced = builder.Advice.IntroduceMethod( builder.Target, nameof(Template) );
                introduced.Name = "IntroducedMethod_ReturnType";
                introduced.ReturnType = introduced.Compilation.TypeFactory.GetTypeByReflectionType( typeof(int) );
            }

            {
                var introduced = builder.Advice.IntroduceMethod( builder.Target, nameof(Template) );
                introduced.Name = "IntroducedMethod_Accessibility";
                introduced.Accessibility = Accessibility.Private;
            }

            {
                var introduced = builder.Advice.IntroduceMethod( builder.Target, nameof(Template) );
                introduced.Name = "IntroducedMethod_IsStatic";
                introduced.IsStatic = true;
            }

            {
                var introduced = builder.Advice.IntroduceMethod( builder.Target, nameof(Template) );
                introduced.Name = "IntroducedMethod_IsVirtual";
                introduced.IsVirtual = true;
            }

            // TODO: Other members.
        }

        [Template]
        public dynamic? Template()
        {
            Console.WriteLine( "This is introduced method." );

            return meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass { }
}