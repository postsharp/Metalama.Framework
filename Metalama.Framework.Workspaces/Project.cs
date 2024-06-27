// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Introspection;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Introspection;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Workspaces
{
    /// <summary>
    /// Represents a C# project for a specific compilation.
    /// </summary>
    public sealed class Project : IProjectSet
    {
        private readonly CompileTimeDomain _domain;
        private readonly ProjectServiceProvider _serviceProvider;
        private readonly WorkspaceProjectOptions _projectOptions;
        private readonly IIntrospectionOptionsProvider? _options;

        internal bool IsMetalamaOutputEvaluated { get; private set; }

        [PublicAPI]
        public string Path { get; }

        internal ICompilation Compilation { get; }

        [PublicAPI]
        public string? TargetFramework => this._projectOptions.TargetFramework;

        internal Project(
            CompileTimeDomain domain,
            ProjectServiceProvider serviceProvider,
            string path,
            ICompilation compilation,
            WorkspaceProjectOptions projectOptions,
            IIntrospectionOptionsProvider? options )
        {
            this._domain = domain;
            this._serviceProvider = serviceProvider;
            this._projectOptions = projectOptions;
            this.Path = path;
            this.Compilation = compilation;
            this._options = options;
        }

        /// <summary>
        /// Gets the set of types defined in the project, including nested types.
        /// </summary>
        [Memo]
        public ImmutableArray<INamedType> Types => this.Compilation.Types.SelectManyRecursive( t => t.Types, includeRoot: true ).ToImmutableArray();

        /// <summary>
        /// Gets the output of Metalama for this project.
        /// </summary>
        [Memo]
        internal IIntrospectionCompilationResult CompilationResult => this.GetCompilationResultsCore();

        private IIntrospectionCompilationResult GetCompilationResultsCore()
        {
            if ( !this._serviceProvider.Global.GetRequiredService<IMetalamaProjectClassifier>()
                    .TryGetMetalamaVersion( this.Compilation.GetRoslynCompilation(), out _ ) )
            {
                // Metalama is not enabled.
                return new NoMetalamaIntrospectionCompilationResult(
                    true,
                    this.Compilation,
                    this.Compilation.GetRoslynCompilation().GetDiagnostics().ToIntrospectionDiagnostics( this.Compilation, IntrospectionDiagnosticSource.CSharp ) );
            }
            else
            {
                var compiler = new IntrospectionCompiler( this._serviceProvider, this._domain, this._options );
                this.IsMetalamaOutputEvaluated = true;

                var result = this._serviceProvider.Global.GetRequiredService<ITaskRunner>().RunSynchronously( () => compiler.CompileAsync( this.Compilation ) );

                return result;
            }
        }

        [PublicAPI]
        [Memo]
        public string Name => System.IO.Path.GetFileNameWithoutExtension( this.Path );

        [Memo]
        private string DisplayName => this.GetDisplayNameCore();

        public override string ToString() => this.DisplayName;

        private string GetDisplayNameCore()
        {
            var name = this.Name;

            if ( !string.IsNullOrEmpty( this.TargetFramework ) )
            {
                return name + " (" + this.TargetFramework + ")";
            }
            else
            {
                return name;
            }
        }

        /// <inheritdoc />
        public ImmutableArray<IIntrospectionDiagnostic> Diagnostics => this.CompilationResult.Diagnostics;

        public ImmutableArray<IIntrospectionAspectLayer> AspectLayers => this.CompilationResult.AspectLayers;

        /// <inheritdoc />
        public ImmutableArray<IIntrospectionAspectInstance> AspectInstances => this.CompilationResult.AspectInstances;

        /// <inheritdoc />
        public ImmutableArray<IIntrospectionAspectClass> AspectClasses => this.CompilationResult.AspectClasses;

        /// <inheritdoc />
        public ImmutableArray<IIntrospectionAdvice> Advice => this.CompilationResult.Advice;

        /// <inheritdoc />
        public ImmutableArray<IIntrospectionTransformation> Transformations => this.CompilationResult.Transformations;

        /// <inheritdoc />
        public bool IsMetalamaEnabled => this._projectOptions.IsFrameworkEnabled;

        /// <inheritdoc />
        public bool HasMetalamaSucceeded => this.CompilationResult.HasMetalamaSucceeded;

        /// <inheritdoc />
        [Memo]
        public ICompilationSet TransformedCode
            => new CompilationSet( $"{this.DisplayName}: transformed code", ImmutableArray.Create( this.CompilationResult.TransformedCode ) );

        /// <inheritdoc />
        ImmutableArray<Project> IProjectSet.Projects => ImmutableArray.Create( this );

        /// <inheritdoc />
        [Memo]
        public ICompilationSet SourceCode => new CompilationSet( $"{this.DisplayName}: source code", ImmutableArray.Create( this.Compilation ) );

        /// <inheritdoc />
        public IProjectSet GetSubset( Predicate<Project> filter ) => throw new NotSupportedException();

        /// <inheritdoc />
        public IDeclaration GetDeclaration( string projectName, string targetFramework, string declarationId, bool metalamaOutput )
            => throw new NotImplementedException();
    }
}