using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Operators.ExistingConflict_Fail
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.IntroduceUnaryOperator(builder.Target, nameof(Template), builder.Target, builder.Target, OperatorKind.UnaryNegation, whenExists: OverrideStrategy.Fail);
            builder.Advice.IntroduceBinaryOperator(builder.Target, nameof(Template), builder.Target, builder.Target, builder.Target, OperatorKind.Addition, whenExists: OverrideStrategy.Fail);
            builder.Advice.IntroduceConversionOperator(builder.Target, nameof(Template), TypeFactory.GetType(typeof(int)), builder.Target, whenExists: OverrideStrategy.Fail);
        }

        [Template]
        public dynamic? Template()
        {
            Console.WriteLine( "This is the introduced operator." );

            return meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass 
    {
        public static TargetClass operator -(TargetClass a)
        {
            Console.WriteLine("This is the original operator.");
            return new TargetClass();
        }

        public static TargetClass operator +(TargetClass a, TargetClass b)
        {
            Console.WriteLine("This is the original operator.");            
            return new TargetClass();
        }

        public static explicit operator TargetClass(int a)
        {
            Console.WriteLine("This is the original operator.");
            return new TargetClass();
        }
    }
}