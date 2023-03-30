#if TEST_OPTIONS
// @RemoveOutputCode
#endif

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Tests.Integration.Tests.Validation.AccessAspectRepository;

#pragma warning disable CS0169, CS8618

public class TheAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Outbound.ValidateReferences( ValidateReference, ReferenceKinds.All );
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
    private C? _f;
}