using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CodeModel.Symbolic
{
    internal interface IHasLocation
    {
        /// <summary>
        /// Gets the location of the code element, to emit diagnostics.
        /// </summary>
        Location? Location { get; }
    }
}