using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Open.Virtuosity
{
    public class VirtualizeAttribute : TypeAspect
    {
        public override void BuildEligibility(IEligibilityBuilder<INamedType> builder)
        {
            base.BuildEligibility(builder);
            builder.MustSatisfy(t => t.TypeKind is TypeKind.Class or TypeKind.RecordClass, t => $"{t} must be class or a record class");
        }
    }
}
