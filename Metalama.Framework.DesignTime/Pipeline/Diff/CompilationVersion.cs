// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Diff
{
    /// <summary>
    /// Computes and stores the changes between the last <see cref="Microsoft.CodeAnalysis.Compilation"/> and a new one.
    /// </summary>
    internal class CompilationVersion : ICompilationVersion
    {
        public DiffStrategy Strategy { get; }

        public ImmutableDictionary<string, SyntaxTreeVersion> SyntaxTrees { get; }

        public ImmutableDictionary<AssemblyIdentity, CompilationReference> References { get; }

        public Compilation Compilation { get; }

        public CompilationVersion(
            DiffStrategy strategy,
            Compilation compilation,
            ImmutableDictionary<string, SyntaxTreeVersion> syntaxTrees,
            ImmutableDictionary<AssemblyIdentity, CompilationReference> references,
            ulong compileTimeProjectHash )
        {
            this.Strategy = strategy;
            this.SyntaxTrees = syntaxTrees;
            this.References = references;
            this.Compilation = compilation;
            this.CompileTimeProjectHash = compileTimeProjectHash;
        }

        /// <summary>
        /// Returns a copy of the current <see cref="CompilationVersion"/> that differs only by the <see cref="Compilation"/> property.
        /// </summary>
        public CompilationVersion WithCompilation( Compilation compilation )
            => new( this.Strategy, compilation, this.SyntaxTrees, this.References, this.CompileTimeProjectHash );

        public static CompilationVersion Create( Compilation compilation, DiffStrategy strategy, CancellationToken cancellationToken = default )
        {
            var references = compilation.ExternalReferences.OfType<CompilationReference>()
                .ToImmutableDictionary( r => r.Compilation.Assembly.Identity, r => r );

            var syntaxTreesBuilder = ImmutableDictionary.CreateBuilder<string, SyntaxTreeVersion>( StringComparer.Ordinal );

            var partialTypesBuilder = ImmutableHashSet.CreateBuilder<TypeDependencyKey>();

            foreach ( var syntaxTree in compilation.SyntaxTrees )
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ( SourceGeneratorHelper.IsGeneratedFile( syntaxTree ) )
                {
                    continue;
                }

                var syntaxTreeVersion = strategy.GetSyntaxTreeVersion( syntaxTree, compilation );

                syntaxTreesBuilder.Add( syntaxTree.FilePath, syntaxTreeVersion );

                foreach ( var partialType in syntaxTreeVersion.PartialTypes )
                {
                    partialTypesBuilder.Add( partialType );
                }
            }

            var syntaxTreeVersions = syntaxTreesBuilder.ToImmutable();

            return new CompilationVersion(
                strategy,
                compilation,
                syntaxTreeVersions,
                references.ToImmutableDictionary(),
                DiffStrategy.ComputeCompileTimeProjectHash( syntaxTreeVersions ) );
        }

        AssemblyIdentity ICompilationVersion.AssemblyIdentity => this.Compilation.AssertNotNull().Assembly.Identity;

        public ulong CompileTimeProjectHash { get; }

        public bool TryGetSyntaxTreeVersion( string path, out SyntaxTreeVersion syntaxTreeVersion )
            => this.SyntaxTrees.AssertNotNull().TryGetValue( path, out syntaxTreeVersion );
    }
}