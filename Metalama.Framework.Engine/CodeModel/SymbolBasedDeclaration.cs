// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Reflection;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;

namespace Metalama.Framework.Engine.CodeModel
{
    public abstract class SymbolBasedDeclaration : BaseDeclaration
    {
        protected SymbolBasedDeclaration( ISymbol symbol )
        {
            Invariant.Assert( symbol.Kind != SymbolKind.ErrorType );
        }

        [Obfuscation( Exclude = true /* The obfuscator believes it implements ISdkDeclaration.Symbol, but it does not. */ )]
        public abstract ISymbol Symbol { get; }

        [Memo]
        public override IDeclaration? ContainingDeclaration => this.Compilation.Factory.GetDeclaration( this.Symbol.ContainingSymbol );

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.Symbol.ToDisplayString( format.ToRoslyn() );

        protected override ISymbol? GetSymbol() => this.Symbol;

        public override Location? DiagnosticLocation => this.Symbol.GetDiagnosticLocation();

        public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => this.Symbol.DeclaringSyntaxReferences;

        public sealed override bool IsImplicitlyDeclared => this.Symbol.IsImplicitlyDeclared;

        public override ImplicitDeclarationKind ImplicitDeclarationKind
            => this.IsImplicitlyDeclared ? ImplicitDeclarationKind.Other : ImplicitDeclarationKind.None;
    }
}