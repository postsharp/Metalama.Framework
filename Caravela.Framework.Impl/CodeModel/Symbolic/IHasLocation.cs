// unset

using Microsoft.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel
{
    internal interface IHasLocation
    {
        /// <summary>
        /// Gets the location of the code element, to emit diagnostics.
        /// </summary>
        Location? Location { get; }
    }
}