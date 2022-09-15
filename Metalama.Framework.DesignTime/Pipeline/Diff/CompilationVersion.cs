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

        public ImmutableDictionary<AssemblyIdentity, ICompilationVersion> ReferencedCompilations { get; }

        public Compilation Compilation { get; }

        public IEnumerable<string> EnumerateSyntaxTreePaths() => this.Compilation.SyntaxTrees.Select( x => x.FilePath );

        /// <summary>
        /// Gets the compilation that should be analyzed by the pipeline. This is typically an older version of
        /// the current <see cref="CompilationVersion"/>, but without the generated syntax trees.
        /// </summary>
        public Compilation CompilationToAnalyze { get; }

        public CompilationVersion(
            DiffStrategy strategy,
            Compilation compilation,
            Compilation compilationToAnalyze,
            ImmutableDictionary<string, SyntaxTreeVersion> syntaxTrees,
            ImmutableDictionary<AssemblyIdentity, ICompilationVersion> referencedCompilations,
            ulong compileTimeProjectHash )
        {
            this.Strategy = strategy;
            this.SyntaxTrees = syntaxTrees;
            this.ReferencedCompilations = referencedCompilations;
            this.Compilation = compilation;
            this.CompilationToAnalyze = compilationToAnalyze;
            this.CompileTimeProjectHash = compileTimeProjectHash;
        }

        /// <summary>
        /// Returns a copy of the current <see cref="CompilationVersion"/> that differs only by the <see cref="Compilation"/> property.
        /// </summary>
        public CompilationVersion WithCompilation( Compilation compilation )
            => new( this.Strategy, compilation, this.CompilationToAnalyze, this.SyntaxTrees, this.ReferencedCompilations, this.CompileTimeProjectHash );

        public static CompilationVersion Create(
            Compilation compilation,
            DiffStrategy strategy,
            ImmutableDictionary<AssemblyIdentity, ICompilationVersion>? referencedCompilations = null, // Can be null for test scenarios.
            CancellationToken cancellationToken = default )
        {
            referencedCompilations ??= ImmutableDictionary<AssemblyIdentity, ICompilationVersion>.Empty;

            var syntaxTreesBuilder = ImmutableDictionary.CreateBuilder<string, SyntaxTreeVersion>( StringComparer.Ordinal );

            var partialTypesBuilder = ImmutableHashSet.CreateBuilder<TypeDependencyKey>();

            var generatedSyntaxTrees = new List<SyntaxTree>();

            foreach ( var syntaxTree in compilation.SyntaxTrees )
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ( SourceGeneratorHelper.IsGeneratedFile( syntaxTree ) )
                {
                    generatedSyntaxTrees.Add( syntaxTree );

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

            var compilationToAnalyze = generatedSyntaxTrees.Count > 0 ? compilation.RemoveSyntaxTrees( generatedSyntaxTrees ) : compilation;

            return new CompilationVersion(
                strategy,
                compilation,
                compilationToAnalyze,
                syntaxTreeVersions,
                referencedCompilations,
                DiffStrategy.ComputeCompileTimeProjectHash( syntaxTreeVersions ) );
        }

        AssemblyIdentity ICompilationVersion.AssemblyIdentity => this.Compilation.AssertNotNull().Assembly.Identity;

        public ulong CompileTimeProjectHash { get; }

        public bool TryGetSyntaxTreeVersion( string path, out SyntaxTreeVersion syntaxTreeVersion )
            => this.SyntaxTrees.AssertNotNull().TryGetValue( path, out syntaxTreeVersion );
    }
}