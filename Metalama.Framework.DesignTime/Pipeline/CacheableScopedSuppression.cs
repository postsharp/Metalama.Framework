// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline
{
    /// <summary>
    /// A compilation-independent version of <see cref="ScopedSuppression"/>, which stores the symbol id instead of the <see cref="ISymbol"/> itself.
    /// </summary>
    internal readonly struct CacheableScopedSuppression
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

            this.SymbolId = suppression.Declaration.GetSymbol()?.GetDocumentationCommentId()
                            ?? throw new AssertionFailedException( $"Cannot get the documentation id of '{suppression.Declaration}'." );
        }

        public override string ToString() => $"{this.Definition.SuppressedDiagnosticId} in {this.SymbolId}";
    }
}