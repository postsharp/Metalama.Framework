using Caravela.Framework.ArchitectureValidation;
using Caravela.Framework.Eligibility;

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