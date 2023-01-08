// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Diff
{
    /// <summary>
    /// The main implementation of <see cref="IProjectVersion"/>.
    /// </summary>
    internal sealed class ProjectVersion : IProjectVersion
    {
        public DiffStrategy Strategy { get; }

        public ImmutableDictionary<string, SyntaxTreeVersion> SyntaxTrees { get; }

        public ImmutableDictionary<ProjectKey, IProjectVersion> ReferencedProjectVersions { get; }

        public ImmutableHashSet<string> ReferencesPortableExecutables { get; }

        public Compilation Compilation { get; }

        /// <summary>
        /// Gets the compilation that should be analyzed by the pipeline. This is typically an older version of
        /// the current <see cref="ProjectVersion"/>, but without the generated syntax trees.
        /// </summary>
        public Compilation CompilationToAnalyze { get; }

        public ProjectVersion(
            DiffStrategy strategy,
            ProjectKey projectKey,
            Compilation compilation,
            Compilation compilationToAnalyze,
            ImmutableDictionary<string, SyntaxTreeVersion> syntaxTrees,
            ImmutableDictionary<ProjectKey, IProjectVersion> referencedCompilations,
            ImmutableHashSet<string> referencesPortableExecutables )
        {
            this.Strategy = strategy;
            this.SyntaxTrees = syntaxTrees;
            this.ReferencedProjectVersions = referencedCompilations;
            this.ReferencesPortableExecutables = referencesPortableExecutables;
            this.Compilation = compilation;
            this.ProjectKey = projectKey;
            this.CompilationToAnalyze = compilationToAnalyze;
        }

        /// <summary>
        /// Returns a copy of the current <see cref="ProjectVersion"/> that differs only by the <see cref="Compilation"/> property.
        /// </summary>
        public ProjectVersion WithCompilation( Compilation compilation )
            => new(
                this.Strategy,
                this.ProjectKey,
                compilation,
                this.CompilationToAnalyze,
                this.SyntaxTrees,
                this.ReferencedProjectVersions,
                this.ReferencesPortableExecutables );

        public static ProjectVersion Create(
            Compilation compilation,
            ProjectKey projectKey,
            DiffStrategy strategy,
            ImmutableDictionary<ProjectKey, IProjectVersion>? referencedCompilations = null, // Can be null for test scenarios.
            ImmutableHashSet<string>? referencesPortableExecutables = null,
            CancellationToken cancellationToken = default )
        {
            referencedCompilations ??= ImmutableDictionary<ProjectKey, IProjectVersion>.Empty;
            referencesPortableExecutables ??= ImmutableHashSet<string>.Empty;

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

            return new ProjectVersion(
                strategy,
                projectKey,
                compilation,
                compilationToAnalyze,
                syntaxTreeVersions,
                referencedCompilations,
                referencesPortableExecutables );
        }

        public ProjectKey ProjectKey { get; }

        public bool TryGetSyntaxTreeVersion( string path, out SyntaxTreeVersion syntaxTreeVersion )
            => this.SyntaxTrees.AssertNotNull().TryGetValue( path, out syntaxTreeVersion );

        public override string ToString() => this.Compilation.AssemblyName ?? this.GetType().Name;
    }
}