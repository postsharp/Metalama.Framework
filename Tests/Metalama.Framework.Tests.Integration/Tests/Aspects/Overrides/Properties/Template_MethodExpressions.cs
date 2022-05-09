using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Template_MethodExpressions
{
    /*
     * Tests expression bodied method templates.
     */

    [AttributeUsage( AttributeTargets.Property )]
    public class TestAttribute : FieldOrPropertyAspect
    {
        public override void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
        {
            builder.Advice.OverrideAccessors( builder.Target, nameof(GetProperty), nameof(SetProperty) );
        }

        [Template]
        public dynamic? GetProperty() => meta.ParseExpression("default");

        [Template]
        public void SetProperty() => Console.WriteLine("Overridden");
    }

    // <target>
    internal class TargetClass
    {
        [Test]
        public int BlockBodiedAccessors
        {
            get
            {
                Console.WriteLine("Original");
                return 42;
            }
            set
            {
                Console.WriteLine("Original");
            }
        }

        [Test]
        public int ExpressionBodiedAccessors
        {
            get => 42;
            set => Console.WriteLine("Original");
        }

        [Test]
        public int ExpressionBodiedProperty => 42;


        [Test]
        public int AutoProperty { get; set; }

        [Test]
        public int AutoGetOnlyProperty { get; }
    }
}