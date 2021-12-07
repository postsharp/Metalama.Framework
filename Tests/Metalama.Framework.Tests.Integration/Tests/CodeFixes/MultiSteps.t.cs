using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.CodeFixes;


namespace Metalama.Framework.Tests.Integration.CodeFixes.MultiSteps
{
  
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    [CompileTime] // TODO: should not be necessary to add [CompileTime]
    public  class NotToStringAttribute : Attribute
    {
    }

    public class ToStringAttribute : TypeAspect
    {
        
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            base.BuildAspect(builder);

            // Suggest to switch to manual implementation.
            if ( builder.AspectInstance.Predecessors[0].Instance is IAttribute attribute  )
            {
                builder.Diagnostics.Suggest(
                    attribute, 
                    CodeFix.Create( codeFixBuilder => this.ImplementManually(codeFixBuilder, builder.Target), "Switch to manual implementation") );
            }
        }

        private async Task ImplementManually(ICodeFixBuilder builder, INamedType targetType)
        {
            await builder.ApplyAspectAsync(targetType, this);
            await builder.RemoveAttributesAsync(targetType, typeof(ToStringAttribute));
            await builder.RemoveAttributesAsync(targetType, typeof(NotToStringAttribute));
        }

        [Introduce(WhenExists = OverrideStrategy.Override, Name = "ToString")]
        public string IntroducedToString()
        {
            // This is not the point.
            throw new NotImplementedException();
            

        }
    }
    internal class MovingVertex
    {
        public double X;

        public double Y;

        public double DX;

        public double DY { get; set; }

        public double Velocity => Math.Sqrt((this.DX * this.DX) + (this.DY * this.DY));


        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
