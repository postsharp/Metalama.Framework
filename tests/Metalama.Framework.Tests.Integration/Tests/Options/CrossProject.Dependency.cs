using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Options;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Project;
using Metalama.Framework.Tests.Integration.Tests.Options.CrossProject;

[assembly: MyOptions( "FromDependencyAssembly" )]

namespace Metalama.Framework.Tests.Integration.Tests.Options.CrossProject;

public record MyOptions : IHierarchicalOptions<IDeclaration>
{
    public string? Value { get; init; }

    public bool? BaseWins { get; init; }

    public IHierarchicalOptions GetDefaultOptions( IProject project ) => this;

    public object ApplyChanges( object changes, in ApplyChangesContext context )
    {
        var other = (MyOptions)overridingObject;

        return new MyOptions { Value = other.Value ?? Value };
    }

    public void BuildEligibility( IEligibilityBuilder<IDeclaration> declaration ) { }
}

[AttributeUsage( AttributeTargets.All, AllowMultiple = true )]
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

[MyOptions( "BaseDeclaringClass" )]
public class BaseNestingClass
{
    public class BaseNestedClass { }
}

public class BaseClassWithoutDirectOptions { }