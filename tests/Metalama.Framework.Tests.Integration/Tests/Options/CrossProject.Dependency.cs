using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Options;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Project;

namespace Metalama.Framework.Tests.Integration.Tests.Options.CrossProject;

public record MyOptions : IHierarchicalOptions<IDeclaration>
{
    public string? Value { get; init; }

    public bool? BaseWins { get; init; }

    public IHierarchicalOptions GetDefaultOptions( IProject project ) => this;

    public object OverrideWith( object options, in HierarchicalOptionsOverrideContext context )
    {
        var other = (MyOptions)options;

        return new MyOptions { Value = other.Value ?? Value };
    }

    public void BuildEligibility( IEligibilityBuilder<IDeclaration> declaration ) { }
}

public class MyOptionsAttribute : Attribute, IHierarchicalOptionsProvider<MyOptions>
{
    private string _value;

    public MyOptionsAttribute( string value )
    {
        _value = value;
    }

    public MyOptions GetOptions() => new() { Value = _value };
}

[MyOptions( "FromBaseClass" )]
public class BaseClass { }