#if TEST_OPTIONS
// @TestScenario(CodeFix)
#endif

using System;
using System.Threading.Tasks;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;

namespace Metalama.Framework.Tests.AspectTests.CodeFixes.MultiSteps
{
    [AttributeUsage( AttributeTargets.Field | AttributeTargets.Property )]
    [RunTimeOrCompileTime] // TODO: should not be necessary to add [CompileTime]
    public class NotToStringAttribute : Attribute { }

    public class ToStringAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            base.BuildAspect( builder );

            // Suggest to switch to manual implementation.
            if (builder.AspectInstance.Predecessors[0].Instance is IAttribute attribute)
            {
                builder.Diagnostics.Suggest(
                    new CodeFix(
                        "Switch to manual implementation",
                        codeFixBuilder => ImplementManually( codeFixBuilder, builder.Target ) ),
                    attribute );
            }
        }

        [CompileTime]
        private async Task ImplementManually( ICodeActionBuilder builder, INamedType targetType )
        {
            await builder.ApplyAspectAsync( targetType, this );
            await builder.RemoveAttributesAsync( targetType, typeof(ToStringAttribute) );
            await builder.RemoveAttributesAsync( targetType, typeof(NotToStringAttribute) );
        }

        [Introduce( WhenExists = OverrideStrategy.Override, Name = "ToString" )]
        public string IntroducedToString()
        {
            // This is not the point.
            throw new NotImplementedException();
        }
    }

    // <target>

    internal class TargetCode
    {
        [ToString]
        internal class MovingVertex
        {
            [NotToString]
            public double X;

            public double Y;

            public double DX;

            public double DY { get; set; }

            public double Velocity => Math.Sqrt( ( DX * DX ) + ( DY * DY ) );
        }
    }
}