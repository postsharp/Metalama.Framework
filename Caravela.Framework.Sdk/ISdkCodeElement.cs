using Caravela.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Sdk
{
    /// <summary>
    /// Extends the user-level <see cref="ICodeElement"/> interface with a <see cref="Symbol"/> property exposing the Roslyn <see cref="ISymbol"/>. 
    /// </summary>
    public interface ISdkCodeElement : ICodeElement
    {
        /// <summary>
        /// Determines if the current code element stems from source code. Returns <c>false</c> if it was introduced by an aspect.
        /// </summary>
        bool IsSourceArtefact { get; }
        
        /// <summary>
        /// Gets the Roslyn <see cref="ISymbol"/> for the current code element, or <c>null</c> if the code element was introduced by an aspect.
        /// </summary>
        ISymbol? Symbol { get; } 
    }
}