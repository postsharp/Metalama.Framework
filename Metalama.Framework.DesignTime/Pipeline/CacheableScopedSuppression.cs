// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline
{
    /// <summary>
    /// A compilation-independent version of <see cref="ScopedSuppression"/>, which stores the symbol id instead of the <see cref="ISymbol"/> itself.
    /// </summary>
    internal sealed class CacheableScopedSuppression : IScopedSuppression
    {
        /// <summary>
        /// Gets the suppression definition.
        /// </summary>
        public SuppressionDefinition Definition { get; }

        ISymbol? IScopedSuppression.GetScopeSymbolOrNull( Compilation compilation ) => this.DeclarationId.ResolveToSymbolOrNull( compilation );

        /// <summary>
        /// Gets the symbol identifier.
        /// </summary>
        public SerializableDeclarationId DeclarationId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheableScopedSuppression"/> struct.
        /// </summary>
        /// <param name="suppression"></param>
        public CacheableScopedSuppression( in ScopedSuppression suppression )
        {
            this.Definition = suppression.Definition;

            this.DeclarationId = suppression.Declaration.ToSerializableId();
        }

        public override string ToString() => $"{this.Definition.SuppressedDiagnosticId} in {this.DeclarationId}";
    }
}