using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.DependencyInjection;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.DependencyInjection;

public class Strategy : IPullStrategy
{
    public PullAction PullFieldOrProperty( IFieldOrProperty fieldOrProperty, IConstructor constructor, in ScopedDiagnosticSink diagnostics )
    {
        var parameterName = fieldOrProperty.Name;
        var parameter = constructor.Parameters.OfName( parameterName );

        if (parameter != null)
        {
            return PullAction.UseExistingParameter( parameter );
        }
        else
        {
            return PullAction.IntroduceParameterAndPull( parameterName, fieldOrProperty.Type );
        }
    }

    public PullAction PullParameter( IParameter parameter, IConstructor constructor, in ScopedDiagnosticSink diagnostics )
    {
        var newParameter = constructor.Parameters.OfName( parameter.Name );

        if (newParameter != null)
        {
            return PullAction.UseExistingParameter( newParameter );
        }
        else
        {
            return PullAction.IntroduceParameterAndPull( parameter.Name, parameter.Type );
        }
    }
}

public class MyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.IntroduceField( builder.Target, "formatter", typeof(ICustomFormatter) );
    }
}