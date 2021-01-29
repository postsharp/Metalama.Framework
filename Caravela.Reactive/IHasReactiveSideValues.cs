namespace Caravela.Reactive
{
    /// <summary>
    /// Exposes a <see cref="SideValues"/> property. This interface must be implemented by the result type of custom operators
    /// that return side values (such as diagnostics). Side values are guaranteed to be copied (combined), from source to result, for all operators.
    /// </summary>
    public interface IHasReactiveSideValues
    {
        /// <summary>
        /// Gets the side values produced by the operator.
        /// </summary>
        ReactiveSideValues SideValues { get; }
    }
}