using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Misc.ChangeTracking
{
    internal class ChangeTrackingAspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            base.BuildAspect( builder );

            foreach (var property in builder.Target.Properties)
            {
                var isSpecifiedProperty = builder.Advice.IntroduceProperty( builder.Target, nameof(IsSpecifiedTemplate) );
                isSpecifiedProperty.Name = $"_is{property.Name}Specified";
                isSpecifiedProperty.Type = TypeFactory.GetType( typeof(bool) );

                builder.Advice.Override(
                    property,
                    nameof(OverrideProperty),
                    tags: new { isSpecifiedProperty = isSpecifiedProperty } );
            }
        }

        [Template]
        public dynamic? OverrideProperty
        {
            get => meta.Proceed();

            set
            {
                var isSpecifiedProperty = (IProperty)meta.Tags["isSpecifiedProperty"]!;
                isSpecifiedProperty.Invokers.Final.SetValue( meta.This, true );
                meta.Proceed();
            }
        }

        [Template]
        public bool IsSpecifiedTemplate { get; private set; }
    }

    // <target>
    [ChangeTrackingAspect]
    internal class MyClass
    {
        public int A { get; set; }

        public string? B { get; set; }
    }
}