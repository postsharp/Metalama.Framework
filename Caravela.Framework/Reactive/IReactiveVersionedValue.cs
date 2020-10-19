namespace Caravela.Reactive
{
    /// <summary>
    /// Exposes a value and a version number.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <remarks></remarks>
    public interface IReactiveVersionedValue<out TValue>
    {
        int Version { get; }
        TValue Value { get; }
    }
}