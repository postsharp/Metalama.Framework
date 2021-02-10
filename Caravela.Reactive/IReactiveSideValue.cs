using System.Diagnostics.CodeAnalysis;

namespace Caravela.Reactive
{
    /// <summary>
    /// Interface to be implemented by side values of reactive operators. Side values must be combinable.
    /// </summary>
    public interface IReactiveSideValue
    {
        /// <summary>
        /// Tries to combine another side value with the current one, if the other is of the proper type.
        /// </summary>
        /// <param name="sideValue">The other side value.</param>
        /// <param name="combinedValue">At output, the combined side value.</param>
        /// <returns><c>true</c> if <paramref name="sideValue"/> was of a supported type, otherwise <c>false</c>.</returns>
        bool TryCombine( IReactiveSideValue sideValue, [NotNullWhen( true )] out IReactiveSideValue? combinedValue );
    }
}