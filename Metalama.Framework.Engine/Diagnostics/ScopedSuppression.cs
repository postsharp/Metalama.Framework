// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Diagnostics
{
    /// <summary>
    /// Represents the suppression of a diagnostic of a given id in a given scope.
    /// </summary>
    public sealed class ScopedSuppression : IScopedSuppression
    {
        public SuppressionDefinition Definition { get; }

        public ISymbol? GetScopeSymbolOrNull( Compilation compilation ) => this.Declaration.GetSymbol();

        public IDeclaration Declaration { get; }

        internal ScopedSuppression( SuppressionDefinition definition, IDeclaration declaration )
        {
            this.Definition = definition;
            this.Declaration = declaration;
        }

        public override string ToString() => $"{this.Definition} in {this.Declaration}";
    }

    public interface IScopedSuppression
    {
        SuppressionDefinition Definition { get; }

        ISymbol? GetScopeSymbolOrNull( Compilation compilation );
    }
}