// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.GenericContexts;
using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Framework.Engine.CodeModel.Visitors;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using SyntaxReference = Microsoft.CodeAnalysis.SyntaxReference;

namespace Metalama.Framework.Engine.CodeModel.Source
{
    public abstract class SymbolBasedDeclaration : BaseDeclaration, ISymbolBasedCompilationElement
    {
        private protected SymbolBasedDeclaration( GenericContext? genericContextForSymbolMapping )
        {
            if ( genericContextForSymbolMapping != null )
            {
                this.GenericContextForSymbolMapping = genericContextForSymbolMapping.IsEmptyOrIdentity ? null : genericContextForSymbolMapping;
            }
        }

        public abstract ISymbol Symbol { get; }

        public GenericContext? GenericContextForSymbolMapping { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Symbol"/> property must be mapped with the <see cref="GenericContext"/>.
        /// Returns <c>false</c> is the symbol is already mapped.
        /// </summary>
        public bool SymbolRequiresMapping => this.GenericContextForSymbolMapping != null;

        [Memo]
        internal sealed override GenericContext GenericContext
            => this.GenericContextForSymbolMapping ?? SymbolGenericContext.Get( this.Symbol, this.GetCompilationContext() );

        [Memo]
        public override IDeclaration? ContainingDeclaration => this.Compilation.Factory.GetDeclaration( this.Symbol.ContainingSymbol );

        public override string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => DisplayStringFormatter.Format( this, format, context );

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
                var isSource = this.Compilation.CompilationContext.SymbolComparer.Equals(
                    this.Symbol.ContainingAssembly,
                    this.Compilation.RoslynCompilation.Assembly );

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
            => other is SymbolBasedDeclaration declaration && this.Compilation.CompilationContext.SymbolComparer.Equals( this.Symbol, declaration.Symbol );

        protected override int GetHashCodeCore() => this.Compilation.CompilationContext.SymbolComparer.GetHashCode( this.Symbol );

        public override bool BelongsToCurrentProject => this.Symbol.ContainingAssembly.Equals( this.Compilation.RoslynCompilation.Assembly );

        [Memo]
        public override ImmutableArray<SourceReference> Sources
            => this.Symbol.DeclaringSyntaxReferences.SelectAsImmutableArray( r => new SourceReference( r.GetSyntax(), SourceReferenceImpl.Instance ) );

        internal override ICompilationElement? Translate(
            CompilationModel newCompilation,
            IGenericContext? genericContext = null,
            Type? interfaceType = null )
        {
            using ( StackOverflowHelper.Detect() )
            {
                var translatedSymbol = newCompilation.CompilationContext.SymbolTranslator.Translate(
                    this.Symbol,
                    symbolCompilationContext: this.Compilation.CompilationContext );

                if ( translatedSymbol == null )
                {
                    return null;
                }

                return newCompilation.Factory.GetCompilationElement( translatedSymbol, genericContext: genericContext );
            }
        }

        internal sealed override DeclarationImplementationKind ImplementationKind => DeclarationImplementationKind.Symbol;
    }
}