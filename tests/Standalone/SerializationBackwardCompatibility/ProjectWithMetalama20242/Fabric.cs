using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Validation;

namespace ProjectWithMetalama20242;

public class Fabric : ProjectFabric
{
    
    public override void AmendProject( IProjectAmender amender ) => 
        amender.SelectReflectionType( typeof(SomeReferencedClass) ).ValidateInboundReferences( this.ValidateReferences, ReferenceGranularity.Member );

    private void ValidateReferences(ReferenceValidationContext context)
    {
        
    }
}