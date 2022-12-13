// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading;
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

        protected override ISymbol GetSymbol() => this.Symbol;

        public override Location? DiagnosticLocation => this.Symbol.GetDiagnosticLocation();

        public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => this.Symbol.DeclaringSyntaxReferences;

        public sealed override bool IsImplicitlyDeclared
            => this.Symbol.IsImplicitlyDeclared ||

               // We consider the Program.Main from top-level statements to be implicit.
               (!this.Symbol.DeclaringSyntaxReferences.IsEmpty && this.Symbol.DeclaringSyntaxReferences[0].GetSyntax() is CompilationUnitSyntax);

        [Memo]
        public override IDeclarationOrigin Origin => this.GetOrigin();

        private IDeclarationOrigin GetOrigin()
        {
            var parentOrigin = this.ContainingDeclaration?.Origin;

            DeclarationOriginKind kind;

            if ( parentOrigin is { IsCompilerGenerated: true } or { Kind: not DeclarationOriginKind.Source } )
            {
                return parentOrigin;
            }
            else if ( parentOrigin != null )
            {
                kind = parentOrigin.Kind;
            }
            else
            {
                var isSource = SymbolEqualityComparer.Default.Equals( this.Symbol.ContainingAssembly, this.Compilation.RoslynCompilation.Assembly );

                if ( isSource )
                {
                    var syntaxTree = this.GetPrimarySyntaxTree();

                    if ( syntaxTree != null )
                    {
                        var isGenerated =
                            this.Compilation.RoslynCompilation.Options.SyntaxTreeOptionsProvider?.IsGenerated( syntaxTree, CancellationToken.None )
                            == GeneratedKind.MarkedGenerated;

                        kind = isGenerated ? DeclarationOriginKind.Source : DeclarationOriginKind.Generator;
                    }
                    else
                    {
                        kind = DeclarationOriginKind.Source;
                    }
                }
                else
                {
                    kind = DeclarationOriginKind.External;
                }
            }

            var isCompilerGenerated = this.Symbol.IsCompilerGenerated();

            return (kind, isCompilerGenerated) switch
            {
                (DeclarationOriginKind.Source, false) => DeclarationOrigin.Source,
                (DeclarationOriginKind.Source, true) => DeclarationOrigin.CompilerGeneratedSource,
                (DeclarationOriginKind.Generator, false) => DeclarationOrigin.Source,
                (DeclarationOriginKind.Generator, true) => DeclarationOrigin.CompilerGeneratedSource,
                (DeclarationOriginKind.External, false) => DeclarationOrigin.External,
                (DeclarationOriginKind.External, true) => DeclarationOrigin.CompilerGeneratedExternal,
                _ => throw new AssertionFailedException( $"Unexpected combination: ({kind}, {isCompilerGenerated})" )
            };
        }

        public override bool Equals( IDeclaration? other )
            => other is SymbolBasedDeclaration declaration && SymbolEqualityComparer.Default.Equals( this.Symbol, declaration.Symbol );

        protected override int GetHashCodeCore() => SymbolEqualityComparer.Default.GetHashCode( this.Symbol );
    }
}