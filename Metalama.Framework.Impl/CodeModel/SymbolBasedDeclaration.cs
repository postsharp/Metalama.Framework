// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Impl.Diagnostics;
using Metalama.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.Impl.CodeModel
{
    internal abstract class SymbolBasedDeclaration : BaseDeclaration
    {
        public abstract ISymbol Symbol { get; }

        [Memo]
        public override IDeclaration? ContainingDeclaration => this.Compilation.Factory.GetDeclaration( this.Symbol.ContainingSymbol );

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.Symbol.ToDisplayString( format.ToRoslyn() );

        protected override ISymbol? GetSymbol() => this.Symbol;

        public override Location? DiagnosticLocation => this.Symbol.GetDiagnosticLocation();

        public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => this.Symbol.DeclaringSyntaxReferences;
    }
}