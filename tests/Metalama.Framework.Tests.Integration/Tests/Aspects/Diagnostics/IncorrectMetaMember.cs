using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Tests.Integration.Aspects.Diagnostics.IncorrectMetaMember;
using System;
using System.Linq;

#pragma warning disable CS0169

[assembly: AspectOrder(typeof(FieldAspectTest), typeof(FieldOrPropertyAspectTest), typeof(IndexerAspectTest), typeof(PropertyAspectTest))]

namespace Metalama.Framework.Tests.Integration.Aspects.Diagnostics.IncorrectMetaMember;

abstract class AspectBase : Aspect, IAspect<IFieldOrPropertyOrIndexer>
{
    public void BuildAspect(IAspectBuilder<IFieldOrPropertyOrIndexer> builder)
    {
        builder.Advice.OverrideAccessors(builder.Target, getTemplate: nameof(Template));
    }

    public void BuildEligibility(IEligibilityBuilder<IFieldOrPropertyOrIndexer> builder) { }

    protected abstract string TargetName { get; }

    [Template]
    dynamic? Template()
    {
        Console.WriteLine(TargetName);

        return meta.Proceed();
    }
}

class FieldAspectTest : AspectBase
{
    [CompileTime]
    protected override string TargetName => meta.Target.Field.Name;
}

class PropertyAspectTest : AspectBase
{
    [CompileTime]
    protected override string TargetName => meta.Target.Property.Name;
}

class FieldOrPropertyAspectTest : AspectBase
{
    [CompileTime]
    protected override string TargetName => meta.Target.FieldOrProperty.Name;
}

class IndexerAspectTest : AspectBase
{
    [CompileTime]
    protected override string TargetName => meta.Target.Indexer.Name;
}

class TargetCode
{
    [FieldAspectTest, PropertyAspectTest, FieldOrPropertyAspectTest, IndexerAspectTest]
    int field;

    [FieldAspectTest, PropertyAspectTest, FieldOrPropertyAspectTest, IndexerAspectTest]
    int property { get; set; }

    [FieldAspectTest, PropertyAspectTest, FieldOrPropertyAspectTest, IndexerAspectTest]
    int this[int i] => 42;
}