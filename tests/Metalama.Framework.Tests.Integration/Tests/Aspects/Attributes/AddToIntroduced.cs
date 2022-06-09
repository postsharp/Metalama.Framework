using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.AddToIntroduced;

[assembly: AspectOrder( typeof(AddAttributeAspect), typeof(IntroducingAspect) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.AddToIntroduced
{
    internal class IntroducingAspect : TypeAspect
    {
        [Introduce]
        private int _field;

        [Introduce]
        private string? Property { get; set; }

        [Introduce]
        private long Method( string p ) => 0;

        [Introduce]
        public event EventHandler? Event;
    }

    internal class AddAttributeAspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            var attribute = AttributeConstruction.Create( typeof(MyAttribute) );

            foreach (var field in builder.Target.Fields)
            {
                builder.Advice.AddAttribute( field, attribute );
            }

            foreach (var property in builder.Target.Properties)
            {
                builder.Advice.AddAttribute( property, attribute );
            }

            foreach (var @event in builder.Target.Events)
            {
                builder.Advice.AddAttribute( @event, attribute );
            }

            foreach (var method in builder.Target.Methods)
            {
                builder.Advice.AddAttribute( method, attribute );
                builder.Advice.AddAttribute( method.ReturnParameter, attribute );

                foreach (var parameter in method.Parameters)
                {
                    builder.Advice.AddAttribute( parameter, attribute );
                }
            }
        }
    }

    internal class MyAttribute : Attribute { }

    // <target>
    [IntroducingAspect]
    [AddAttributeAspect]
    internal class C { }
}