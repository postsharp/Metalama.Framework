using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Aspects.Initialization.TypeConstructing_MemberAndType;

[assembly: AspectOrder( typeof(Aspect2), typeof(Aspect1) )]

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.TypeConstructing_MemberAndType
{
    public class AspectBase : TypeAspect
    {
        [Template]
        public void Template()
        {
            var targetConstructorString = meta.Target.Constructor.ToDisplayString(CodeDisplayFormat.MinimallyQualified);
            var targetDeclarationString = ((IDeclaration)meta.Tags["target"]!).ToDisplayString(CodeDisplayFormat.MinimallyQualified);
            Console.WriteLine($"{targetConstructorString}, {targetDeclarationString}: {meta.AspectInstance.AspectClass.ShortName}");
        }
    }

    public class Aspect1 : AspectBase
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advices.AddInitializerBeforeTypeConstructor(
                builder.Target,
                nameof(Template),
                tags: new TagDictionary() { ["target"] = builder.Target });
        }
    }

    public class Aspect2 : AspectBase
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advices.AddInitializerBeforeTypeConstructor(
                builder.Target.Properties.First(),
                nameof(Template),
                tags: new TagDictionary() { ["target"] = builder.Target.Properties.First() });
        }
    }

    // <target>
    [Aspect1]
    [Aspect2]
    public class TargetCode
    {
        static TargetCode()
        {
        }

        public static int Foo { get; }
    }
}