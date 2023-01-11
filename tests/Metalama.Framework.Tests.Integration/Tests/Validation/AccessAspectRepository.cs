using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Tests.Integration.Tests.Validation.AccessAspectRepository;

public class TheAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.With( t => t ).ValidateReferences( ValidateReference, ReferenceKinds.All );
    }

    private void ValidateReference( in ReferenceValidationContext context )
    {
        if (!( (INamedType)context.ReferencedDeclaration ).Enhancements().HasAspect<TheAspect>())
        {
            throw new InvalidOperationException();
        }
    }
}

[TheAspect]
internal class C
{
    private C _f;
}