// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Introspection;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Introspection;
using Metalama.Framework.Project;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Workspaces
{
    /// <summary>
    /// Represents a C# project for a specific compilation.
    /// </summary>
    public sealed class Project : IProjectSet
    {
        private readonly CompileTimeDomain _domain;
        private readonly ServiceProvider _serviceProvider;
        private readonly IIntrospectionOptionsProvider? _options;

        internal bool IsMetalamaOutputEvaluated { get; private set; }

        public string Path { get; }

        internal ICompilation Compilation { get; }

        public string TargetFramework { get; }

        internal Project(
            CompileTimeDomain domain,
            ServiceProvider serviceProvider,
            string path,
            ICompilation compilation,
            string? targetFramework,
            IIntrospectionOptionsProvider? options )
        {
            this._domain = domain;
            this._serviceProvider = serviceProvider;
            this.Path = path;
            this.Compilation = compilation;
            this._options = options;
            this.TargetFramework = targetFramework ?? "";
        }

        /// <summary>
        /// Gets the set of types defined in the project, including nested types.
        /// </summary>
        [Memo]
        public ImmutableArray<INamedType> Types => this.Compilation.Types.SelectManyRecursive( t => t.NestedTypes ).ToImmutableArray();

        /// <summary>
        /// Gets the output of Metalama for this project.
        /// </summary>
        [Memo]
        internal IIntrospectionCompilationResult CompilationResult => this.GetCompilationResultsCore();

        private IIntrospectionCompilationResult GetCompilationResultsCore()
        {
            if ( !this._serviceProvider.GetRequiredService<IMetalamaProjectClassifier>().IsMetalamaEnabled( this.Compilation.GetRoslynCompilation() ) )
            {
                // Metalama is not enabled.
                return new NoMetalamaIntrospectionCompilationResult(
                    true,
                    this.Compilation,
                    this.Compilation.GetRoslynCompilation().GetDiagnostics().ToIntrospectionDiagnostics( this.Compilation, DiagnosticSource.CSharp ) );
            }
            else
            {
                var compiler = new IntrospectionCompiler( this._domain, this._serviceProvider, this._options );
                this.IsMetalamaOutputEvaluated = true;

                var result = TaskHelper.RunAndWait( () => compiler.CompileAsync( this.Compilation ) );

                return result;
            }
        }

        public override string ToString()
        {
            var name = System.IO.Path.GetFileNameWithoutExtension( this.Path );

            if ( !string.IsNullOrEmpty( this.TargetFramework ) )
            {
                return name + "(" + this.TargetFramework + ")";
            }
            else
            {
                return name;
            }
        }

        /// <inheritdoc />
        public ImmutableArray<IIntrospectionDiagnostic> Diagnostics => this.CompilationResult.Diagnostics;

        /// <inheritdoc />
        public ImmutableArray<IIntrospectionAspectInstance> AspectInstances => this.CompilationResult.AspectInstances;

        /// <inheritdoc />
        public ImmutableArray<IIntrospectionAspectClass> AspectClasses => this.CompilationResult.AspectClasses;

        /// <inheritdoc />
        public ImmutableArray<IIntrospectionAdvice> Advice => this.CompilationResult.Advice;

        /// <inheritdoc />
        public ImmutableArray<IIntrospectionTransformation> Transformations => this.CompilationResult.Transformations;

        /// <inheritdoc />
        public bool IsMetalamaEnabled => this.CompilationResult.IsMetalamaEnabled;

        /// <inheritdoc />
        public bool IsMetalamaSuccessful => this.CompilationResult.IsMetalamaSuccessful;

        /// <inheritdoc />
        [Memo]
        public ICompilationSet TransformedCode => new CompilationSet( this.Path, ImmutableArray.Create( this.CompilationResult.TransformedCode ) );

        /// <inheritdoc />
        ImmutableArray<Project> IProjectSet.Projects => ImmutableArray.Create( this );

        /// <inheritdoc />
        [Memo]
        public ICompilationSet SourceCode => new CompilationSet( this.Path, ImmutableArray.Create( this.Compilation ) );

        /// <inheritdoc />
        public IProjectSet GetSubset( Predicate<Project> filter ) => throw new NotSupportedException();

        /// <inheritdoc />
        public IDeclaration? GetDeclaration( string projectPath, string targetFramework, string declarationId, bool metalamaOutput )
            => throw new NotImplementedException();
    }
}