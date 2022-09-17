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
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Metalama.Framework.Workspaces
{
    /// <summary>
    /// Represents a C# project for a specific compilation.
    /// </summary>
    public sealed class Project
    {
        private readonly CompileTimeDomain _domain;
        private readonly ServiceProvider _serviceProvider;

        internal bool IsMetalamaOutputEvaluated { get; private set; }

        public string Path { get; }

        public ICompilation Compilation { get; }

        public string TargetFramework { get; }

        internal Project( CompileTimeDomain domain, ServiceProvider serviceProvider, string path, ICompilation compilation, string? targetFramework )
        {
            this._domain = domain;
            this._serviceProvider = serviceProvider;
            this.Path = path;
            this.Compilation = compilation;
            this.TargetFramework = targetFramework ?? "";
        }

        [Memo]
        public ImmutableArray<IIntrospectionDiagnostic> SourceDiagnostics
            => this.Compilation.GetRoslynCompilation().GetDiagnostics().ToReportedDiagnostics( this.Compilation, DiagnosticSource.CSharp );

        /// <summary>
        /// Gets the set of types defined in the project, including nested types.
        /// </summary>
        [Memo]
        public ImmutableArray<INamedType> Types => this.Compilation.Types.SelectManyRecursive( t => t.NestedTypes ).ToImmutableArray();

        /// <summary>
        /// Gets the output of Metalama for this project.
        /// </summary>
        [Memo]
        public IIntrospectionCompilationOutput MetalamaOutput => this.ApplyMetalama();

        private IIntrospectionCompilationOutput ApplyMetalama()
        {
            var compiler = new IntrospectionCompiler( this._domain );
            this.IsMetalamaOutputEvaluated = true;

            var result = TaskHelper.RunAndWait( () => compiler.CompileAsync( this.Compilation, this._serviceProvider ) );

            return result;
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
    }
}