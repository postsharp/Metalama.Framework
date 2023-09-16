using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Options;
using Metalama.Framework.Project;
using Metalama.Framework.Tests.Integration.Tests.Options;

[assembly: AspectOrder( typeof(OptionsAspect), typeof(ModifyOptionsAspect) )]

namespace Metalama.Framework.Tests.Integration.Tests.Options;

public record MyOptions : IAspectOptions<IDeclaration>
{
    public string? Value { get; init; }

    public bool? BaseWins { get; init; }

    public IAspectOptions GetDefaultOptions( IProject project ) => this;

    public object OverrideWith( object options, in AspectOptionsOverrideContext context )
    {
        if (BaseWins.GetValueOrDefault() && context.Axis == AspectOptionsOverrideAxis.ContainmentOverBase)
        {
            return this;
        }
        else
        {
            var other = (MyOptions)options;

            return new MyOptions { Value = other.Value ?? Value, BaseWins = other.BaseWins ?? BaseWins };
        }
    }

    public void BuildEligibility( IEligibilityBuilder<IDeclaration> declaration ) { }
}

public class MyOptionsAttribute : Attribute, IAspectOptionsAttribute<MyOptions>
{
    private string _value;
    private bool _baseWins;

    public MyOptionsAttribute( string value, bool baseWins = false )
    {
        _value = value;
        _baseWins = baseWins;
    }

    public MyOptions ToOptions() => new() { Value = _value, BaseWins = _baseWins };
}

public class ActualOptionsAttribute : Attribute
{
    public ActualOptionsAttribute( string value ) { }
}

public class OptionsAspect : Attribute, IAspect<IDeclaration>
{
    public void BuildAspect( IAspectBuilder<IDeclaration> builder )
    {
        builder.Advice.IntroduceAttribute(
            builder.Target,
            AttributeConstruction.Create( typeof(ActualOptionsAttribute), new[] { builder.Target.Enhancements().GetOptions<MyOptions>().Value } ) );
    }

    public void BuildEligibility( IEligibilityBuilder<IDeclaration> builder ) { }
}

public class ModifyOptionsAspect : Attribute, IAspect<IDeclaration>
{
    private string _value;

    public ModifyOptionsAspect( string value )
    {
        _value = value;
    }

    public void BuildAspect( IAspectBuilder<IDeclaration> builder )
    {
        // Get the old value so populate the cache and we can test that the cache is invalidated.
        var oldValue = builder.Target.Enhancements().GetOptions<MyOptions>().Value;
        builder.Outbound.Configure( x => new MyOptions { Value = $"{oldValue}->{_value}" } );
    }

    public void BuildEligibility( IEligibilityBuilder<IDeclaration> builder ) { }
}