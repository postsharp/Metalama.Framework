using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.ParameterAspect;

public class RequiredAttribute : Attribute, IAspect<IParameter>
{
    public void BuildEligibility( IEligibilityBuilder<IParameter> builder ) { }

    public void BuildAspect( IAspectBuilder<IParameter> builder )
    {
        builder.Advice.Override( (IMethod)builder.Target.DeclaringMember, nameof(Template), tags: new { ParameterName = builder.Target.Name } );
    }

    [Template]
    private dynamic? Template()
    {
        var parameterName = (string)meta.Tags["ParameterName"]!;
        var parameter = meta.ParseExpression( parameterName );

        if (parameter.Value == null)
        {
            throw new ArgumentNullException( parameterName );
        }

        return meta.Proceed();
    }
}

// <target>
internal class Class
{
    private void M( [Required] object? a, [Required] object? b ) { }
}