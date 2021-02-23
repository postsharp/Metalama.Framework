// unset

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Impl
{
    /// <summary>
    /// Priorities of <see cref="IAspectSource"/>, which determine in which order the sources must be evaluated.
    /// </summary>
    internal enum AspectSourcePriority
    {
        // First (lower) priorities are evaluated first. Order matters.

        /// <summary>
        /// Aspects added because of aspect inheritance.
        /// </summary>
        Inherited,

        /// <summary>
        /// Exclusions.
        /// </summary>
        Exclusion,

        /// <summary>
        /// Aspects defined by custom attributes.
        /// </summary>
        FromAttribute,

        /// <summary>
        /// Provided implicitly by <see cref="RequiresAspectAttribute"/>.
        /// </summary>
        Implicit,

        /// <summary>
        /// A source that performs aggregation of aspects and does not need to be aggregated again.
        /// </summary>
        Aggregate
    }
}