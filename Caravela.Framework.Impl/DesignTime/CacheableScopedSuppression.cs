// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Sdk;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.DesignTime
{
    /// <summary>
    /// A compilation-independent version of <see cref="ScopedSuppression"/>, which stores the symbol id instead of the <see cref="ISymbol"/> itself.
    /// </summary>
    internal readonly struct CacheableScopedSuppression
    {
        
        /// <summary>
        /// Gets the diagnostic identifier.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the symbol identifier.
        /// </summary>
        public string SymbolId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheableScopedSuppression"/> struct.
        /// </summary>
        /// <param name="suppression"></param>
        public CacheableScopedSuppression( in ScopedSuppression suppression )
        {
            this.Id = suppression.Id;
            this.SymbolId = suppression.CodeElement.GetSymbol()?.GetDocumentationCommentId() ?? throw new AssertionFailedException();
        }

        public override string ToString() => $"{this.Id} in {this.SymbolId}";
    }
}