using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Constructors.Visibility;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.IntroduceConstructor(
            builder.Target,
            nameof(Template),
            buildConstructor: c =>
            {
                c.AddParameter("a", typeof(byte));
                c.AddParameter("b", typeof(string), defaultValue: TypedConstant.Create("Private"));
                c.Accessibility = Accessibility.Private;
            });

        builder.Advice.IntroduceConstructor(
            builder.Target,
            nameof(Template),
            buildConstructor: c =>
            {
                c.AddParameter("a", typeof(short));
                c.AddParameter("b", typeof(string), defaultValue: TypedConstant.Create("ProtectedInternal"));
                c.Accessibility = Accessibility.ProtectedInternal;
            });

        builder.Advice.IntroduceConstructor(
            builder.Target,
            nameof(Template),
            buildConstructor: c =>
            {
                c.AddParameter("a", typeof(int));
                c.AddParameter("b", typeof(string), defaultValue: TypedConstant.Create("PrivateProtected"));
                c.Accessibility = Accessibility.PrivateProtected;
            });

        builder.Advice.IntroduceConstructor(
            builder.Target,
            nameof(Template),
            buildConstructor: c =>
            {
                c.AddParameter("a", typeof(long));
                c.AddParameter("b", typeof(string), defaultValue: TypedConstant.Create("Internal"));
                c.Accessibility = Accessibility.Internal;
            });

        builder.Advice.IntroduceConstructor(
            builder.Target,
            nameof(Template),
            buildConstructor: c =>
            {
                c.AddParameter("a", typeof(float));
                c.AddParameter("b", typeof(string), defaultValue: TypedConstant.Create("Protected"));
                c.Accessibility = Accessibility.Protected;
            });

        builder.Advice.IntroduceConstructor(
            builder.Target,
            nameof(Template),
            buildConstructor: c =>
            {
                c.AddParameter("a", typeof(double));
                c.AddParameter("b", typeof(string), defaultValue: TypedConstant.Create("Public"));
                c.Accessibility = Accessibility.Public;
            });
    }

    [Template]
    public void Template()
    {
        Console.WriteLine( "This is introduced constructor." );
    }
}

// <target>
[Introduction]
internal class TargetClass { }