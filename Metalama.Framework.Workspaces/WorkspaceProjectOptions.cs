// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.Options;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.Workspaces
{
    /// <summary>
    /// The implement of <see cref="IProjectOptions"/> used by <see cref="Workspace"/>.
    /// </summary>
    internal class WorkspaceProjectOptions : MSBuildProjectOptions
    {
        private readonly Microsoft.CodeAnalysis.Project _roslynProject;
        private readonly Compilation _compilation;

        public WorkspaceProjectOptions(
            Microsoft.CodeAnalysis.Project roslynProject,
            Microsoft.Build.Evaluation.Project msbuildProject,
            Compilation compilation ) : base( new PropertySource( msbuildProject ), ImmutableArray<object>.Empty, TransformerOptions.Default )
        {
            this._roslynProject = roslynProject;
            this._compilation = compilation;
        }

        public override string? AssemblyName => this._compilation.AssemblyName;

        public override bool FormatOutput => true;

        public override bool FormatCompileTimeCode => true;

        public override string? ProjectPath => this._roslynProject.FilePath;

        public override bool IsDesignTimeEnabled => false;

        public static string? GetTargetFrameworkFromRoslynProject( Microsoft.CodeAnalysis.Project roslynProject )
        {
            if ( roslynProject.Name.EndsWith( ')' ) )
            {
                var indexOfParenthesis = roslynProject.Name.LastIndexOf( '(' );
                var targetFramework = roslynProject.Name.Substring( indexOfParenthesis + 1, roslynProject.Name.Length - indexOfParenthesis - 2 );

                return targetFramework;
            }
            else
            {
                return null;
            }
        }

        private class PropertySource : IProjectOptionsSource
        {
            private readonly Microsoft.Build.Evaluation.Project _msbuildProject;

            public PropertySource( Microsoft.Build.Evaluation.Project msbuildProject )
            {
                this._msbuildProject = msbuildProject;
            }

            public bool TryGetValue( string name, out string? value )
            {
                value = this._msbuildProject.GetProperty( name )?.EvaluatedValue;

                return !string.IsNullOrEmpty( value );
            }
        }
    }
}