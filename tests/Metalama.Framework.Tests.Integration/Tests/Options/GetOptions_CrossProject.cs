using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Options;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Project;

namespace Metalama.Framework.Tests.Integration.Tests.Options.GetOptions_CrossProject;

public record Options : IHierarchicalOptions<INamedType>
{
    public IHierarchicalOptions GetDefaultOptions( OptionsInitializationContext context ) => this;

    public object ApplyChanges( object changes, in ApplyChangesContext context )
    {
        return this;
    }
}

class Aspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        var externalType = (INamedType)TypeFactory.GetType( typeof(C) );

        var options = externalType.Enhancements().GetOptions<Options>();

        if (options is null)
            throw new Exception();
    }
}

// <target>
[Aspect]
class Target
{
    
}