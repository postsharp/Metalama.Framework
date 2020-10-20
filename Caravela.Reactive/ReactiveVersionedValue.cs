using System.Collections.Immutable;

namespace Caravela.Reactive
{
    /// <summary>
    /// Represents a versioned value. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// This type is a class and not a struct to enforce atomic change its values.
    /// </remarks>
    public sealed class ReactiveVersionedValue<T> : IReactiveVersionedValue<T>
    {

        public ReactiveVersionedValue( T value, int version, ReactiveSideValues sideValues = default )
        {
            this.Version = version;
            this.Value = value;
            this.SideValues = sideValues;
        }

        public int Version { get; }
        public T Value { get; }
        public ReactiveSideValues SideValues { get; }

    }
}