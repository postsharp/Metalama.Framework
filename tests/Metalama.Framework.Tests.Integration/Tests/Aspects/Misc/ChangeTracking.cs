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
                // TODO: IAdviceResult.Declaration should not expose the Builder but the Built thing of the mutable model?
                
                var isSpecifiedProperty = builder.Advice.IntroduceProperty(
                        builder.Target,
                        nameof(IsSpecifiedTemplate),
                        buildProperty: p =>
                        {
                            p.Name = $"_is{property.Name}Specified";
                            p.Type = TypeFactory.GetType( typeof(bool) );
                        } )
                    .Declaration;

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
                isSpecifiedProperty.Value =  true ;
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