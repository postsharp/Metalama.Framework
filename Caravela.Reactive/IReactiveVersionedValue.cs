namespace Caravela.Reactive
{
    /// <summary>
    /// Exposes a value and a version number.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IReactiveVersionedValue<out TValue> : IHasReactiveSideValues
    {
        /// <summary>
        /// Gets the version of the <see cref="Value"/>.
        /// </summary>
        int Version { get; }

        /// <summary>
        /// Gets the value itself.
        /// </summary>
        TValue Value { get; }
    }
}