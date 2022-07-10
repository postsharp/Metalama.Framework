using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.Programmatic_CrossAssembly
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.IntroduceUnaryOperator(builder.Target, nameof(Template), builder.Target, builder.Target, OperatorKind.UnaryNegation);
            builder.Advice.IntroduceBinaryOperator(builder.Target, nameof(Template), builder.Target, TypeFactory.GetType(typeof(int)), builder.Target, OperatorKind.Addition);
            builder.Advice.IntroduceConversionOperator(builder.Target, nameof(Template), TypeFactory.GetType(typeof(int)), builder.Target);
        }

        [Template]
        public dynamic? Template()
        {
            Console.WriteLine( "This is the introduced operator." );

            return meta.Proceed();
        }
    }
}