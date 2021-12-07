// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Diagnostics;
using Metalama.Framework.Impl.CodeModel;
using Metalama.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Impl.DesignTime.Pipeline
{
    /// <summary>
    /// A compilation-independent version of <see cref="ScopedSuppression"/>, which stores the symbol id instead of the <see cref="ISymbol"/> itself.
    /// </summary>
    public readonly struct CacheableScopedSuppression
    {
        /// <summary>
        /// Gets the suppression definition.
        /// </summary>
        public SuppressionDefinition Definition { get; }

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
            this.Definition = suppression.Definition;
            this.SymbolId = suppression.Declaration.GetSymbol()?.GetDocumentationCommentId() ?? throw new AssertionFailedException();
        }

        public override string ToString() => $"{this.Definition.SuppressedDiagnosticId} in {this.SymbolId}";
    }
}