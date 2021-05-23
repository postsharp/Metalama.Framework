using Caravela.Framework.Eligibility;
using Caravela.Framework.Validation;

namespace Caravela.Framework.Aspects
{
    [InternalImplement]
    public interface IAspectMarkerTypeBuilder<out T>
    {
        string DisplayName { get; set; }

        string? Description { get; set; }

        IEligibilityBuilder<T> Eligibility { get; }
    }
}