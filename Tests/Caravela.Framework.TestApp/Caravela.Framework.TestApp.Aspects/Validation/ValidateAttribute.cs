using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;

namespace Caravela.Framework.TestApp.Aspects.Validation
{
    // This file is a demo use of aspect markers and template subroutines.

    public abstract class ValidateAttribute : Attribute, IAspectMarker<IParameter, IMethod, ValidateAspect>
    {
        public virtual void BuildEligibility(IEligibilityBuilder<IParameter> builder) { }

        public virtual int Priority { get; }

        // This is a template subroutine.
        public abstract void Validate(string name, dynamic value);
    }
}
