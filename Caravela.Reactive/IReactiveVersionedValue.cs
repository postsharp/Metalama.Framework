namespace Caravela.Reactive
{
    /// <summary>
    /// Exposes a value and a version number.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <remarks></remarks>
    public interface IReactiveVersionedValue<out TValue> : IHasReactiveSideValues
    {
        int Version { get; }

        TValue Value { get; }


    }
}