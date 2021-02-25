using System;
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
        /// Gets the Roslyn <see cref="ISymbol"/> for the current code element, or throws <see cref="NotSupportedException"/>
        /// if <see cref="ICodeElement.Origin"/> is <see cref="CodeOrigin.Aspect"/>. Note that the symbol returned can be linked to a different
        /// Roslyn compilation than the one provided to the aspect weaver.
        /// </summary>
        ISymbol Symbol { get; }
    }
}