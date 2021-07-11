using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.Aspects.Initialize.TwoIAspectImplementations
{
    public class LogAttribute : Attribute, IAspect<IMethod>, IAspect<IFieldOrProperty>
    {
        public void BuildAspect(IAspectBuilder<IMethod> builder ) 
        {
            builder.AdviceFactory.OverrideMethod(builder.TargetDeclaration, nameof(this.OverrideMethod));
        }

        public void BuildAspect(IAspectBuilder<IFieldOrProperty> builder) 
        {
            builder.AdviceFactory.OverrideFieldOrProperty(builder.TargetDeclaration, nameof(this.OverrideProperty));
        }

        [Template]
        private dynamic? OverrideMethod()
        {
            Console.WriteLine("Entering " + meta.Method.ToDisplayString());
            try
            {
                return meta.Proceed();
            }
            finally
            {
                Console.WriteLine("Leaving " + meta.Method.ToDisplayString());
            }
        }

        [Template]
        private dynamic? OverrideProperty
        {
            get => meta.Proceed();

            set
            {
                Console.WriteLine("Assigning " + meta.Method.ToDisplayString());
                meta.Proceed();
            }
        }
    }
    
    // <target>
    class TargetCode 
    {
        [Log]
        public int Method(int a, int b)
        {
            return a + b;
        }

        [Log]
        public int Property { get; set; }

        [Log]
        public string? Field { get; set; }
    }
}