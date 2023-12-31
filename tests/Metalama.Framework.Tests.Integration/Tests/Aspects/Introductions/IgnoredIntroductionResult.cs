using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Engine.Utilities.Diagnostics;
using System;
using System.IO;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.IgnoredIntroductionResult;

#pragma warning disable CS0067, CS0169, CS0649

internal class Aspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        base.BuildAspect(builder);

        AssertResult(builder.Advice.IntroduceField(
            builder.Target,
            nameof(_field),
            whenExists: OverrideStrategy.Ignore));

        AssertResult(builder.Advice.IntroduceProperty(
            builder.Target,
            nameof(Property),
            whenExists: OverrideStrategy.Ignore));

        AssertResult(builder.Advice.IntroduceIndexer(
            builder.Target,
            typeof(int),
            nameof(IndexerGet),
            null,
            whenExists: OverrideStrategy.Ignore));

        AssertResult(builder.Advice.IntroduceEvent(
            builder.Target,
            nameof(Event),
            whenExists: OverrideStrategy.Ignore));

        AssertResult(builder.Advice.IntroduceUnaryOperator(
            builder.Target,
            nameof(OperatorMinus),
            builder.Target,
            builder.Target,
            OperatorKind.UnaryNegation,
            whenExists: OverrideStrategy.Ignore));

        AssertResult(builder.Advice.IntroduceMethod(
            builder.Target,
            nameof(CloneMethod),
            whenExists: OverrideStrategy.Ignore,
            buildMethod: method => method.Name = nameof(Clone)));

        AssertResult(builder.Advice.IntroduceAttribute(
            builder.Target,
            AttributeConstruction.Create(typeof(Aspect)),
            OverrideStrategy.Ignore));

        var implementInterfaceResult = builder.Advice.ImplementInterface(builder.Target, typeof(ICloneable), OverrideStrategy.Ignore);

        if (implementInterfaceResult.Outcome != AdviceOutcome.Ignore)
        {
            throw new Exception("Advice wasn't ignored.");
        }

        if (implementInterfaceResult.InterfaceMembers.Count != 1)
        {
            throw new Exception($"Expected 1 implemented interface member, got {implementInterfaceResult.InterfaceMembers.Count}.");
        }
    }

    static void AssertResult(IIntroductionAdviceResult<IDeclaration> result)
    {
        if (result.Outcome != AdviceOutcome.Ignore)
        {
            throw new Exception("Advice wasn't ignored.");
        }

        if (result.Declaration == null)
        {
            throw new Exception("Original declaration wasn't provided.");
        }
    }

    [Template]
    private int _field;

    [Template]
    int Property { get; set; }

    [Template]
    int IndexerGet(int i) => 42;

    [Template]
    event EventHandler? Event;

    [Template]
    public static Target OperatorMinus(Target target) => target;

    [Template]
    public object CloneMethod() => new Target();

    [InterfaceMember(WhenExists = InterfaceMemberOverrideStrategy.Ignore)]
    public object Clone() => new Target();
}

// <target>
[Aspect]
class Target : ICloneable
{
    int _field;
    int Property { get; set; }
    int this[int i] => 42;
    event EventHandler? Event;
    public static Target operator -(Target target) => target;

    public object Clone() => new Target();

    ~Target() { }
}