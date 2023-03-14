using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using System;
using System.Linq;

#pragma warning disable CS0169

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
    dynamic Template()
    {
        Console.WriteLine(TargetName);

        return meta.Proceed();
    }
}

class FieldAspect : AspectBase
{
    [CompileTime]
    protected override string TargetName => meta.Target.Field.Name;
}

class PropertyAspect : AspectBase
{
    [CompileTime]
    protected override string TargetName => meta.Target.Property.Name;
}

class FieldOrPropertyAspect : AspectBase
{
    [CompileTime]
    protected override string TargetName => meta.Target.FieldOrProperty.Name;
}

class IndexerAspect : AspectBase
{
    [CompileTime]
    protected override string TargetName => meta.Target.Indexer.Name;
}

class TargetCode
{
    [FieldAspect, PropertyAspect, FieldOrPropertyAspect, IndexerAspect]
    int field;

    [FieldAspect, PropertyAspect, FieldOrPropertyAspect, IndexerAspect]
    int property { get; set; }

    [FieldAspect, PropertyAspect, FieldOrPropertyAspect, IndexerAspect]
    int this[int i] => 42;
}