using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Sdk
{
    /// <summary>
    /// Extends the user-level <see cref="IType"/> interface with a <see cref="TypeSymbol"/> exposing the Roslyn <see cref="ITypeSymbol"/>.
    /// </summary>
    public interface ISdkType : IType
    {
        /// <summary>
        /// Gets the <see cref="ITypeSymbol"/> for the current type.
        /// </summary>
        ITypeSymbol? TypeSymbol { get; }
    }
}