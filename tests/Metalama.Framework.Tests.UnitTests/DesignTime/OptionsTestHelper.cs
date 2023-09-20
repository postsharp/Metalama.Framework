namespace Metalama.Framework.Tests.UnitTests.DesignTime;

internal static class OptionsTestHelper
{
    public const string Code =
        """
        using System;
        using Metalama.Framework.Aspects;
        using Metalama.Framework.Code;
        using Metalama.Framework.Options;
        using Metalama.Framework.Eligibility;
        using Metalama.Framework.Project;

        public record MyOptions : IHierarchicalOptions<IDeclaration>
        {
            public string? Value { get; set; }
        
            public IHierarchicalOptions GetDefaultOptions( IProject project ) => this;
        
            public object OverrideWith( object overridingObject, in OverrideContext context )
            {
                var other = (MyOptions)overridingObject;
        
                return new MyOptions { Value = other.Value ?? Value };
            }
        
            public void BuildEligibility( IEligibilityBuilder<IDeclaration> declaration ) { }
        }
                               
        """;
}