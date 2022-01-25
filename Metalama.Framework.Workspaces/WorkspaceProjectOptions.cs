// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Options;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Workspaces
{
    /// <summary>
    /// The implement of <see cref="IProjectOptions"/> used by <see cref="Workspace"/>.
    /// </summary>
    internal class WorkspaceProjectOptions : IProjectOptions
    {
        private readonly Microsoft.CodeAnalysis.Project _roslynProject;
        private readonly Compilation _compilation;
        private readonly ImmutableDictionary<string, string> _properties;

        public WorkspaceProjectOptions(
            Microsoft.CodeAnalysis.Project roslynProject,
            Microsoft.Build.Evaluation.Project msbuildProject,
            Compilation compilation,
            string? targetFramework )
        {
            var compilerVisibleProperties = msbuildProject.Items.Where( i => i.ItemType == "CompilerVisibleProperty" ).Select( i => i.EvaluatedInclude );

            this._properties = compilerVisibleProperties.Select( p => msbuildProject.Properties.FirstOrDefault( x => x.Name == p ) )
                .WhereNotNull()
                .ToImmutableDictionary( x => x.Name, x => x.EvaluatedValue );

            this._roslynProject = roslynProject;
            this._compilation = compilation;
            this.TargetFramework = targetFramework;
            this.Configuration = msbuildProject.Properties.FirstOrDefault( p => p.Name == "Configuration" )?.EvaluatedValue;
        }

        public string ProjectId { get; } = Guid.NewGuid().ToString();

        public string? BuildTouchFile => null;

        public string? AssemblyName => this._compilation.AssemblyName;

        public ImmutableArray<object> PlugIns => ImmutableArray<object>.Empty;

        public bool IsFrameworkEnabled => true;

        public bool FormatOutput => true;

        public bool FormatCompileTimeCode => true;

        public bool IsUserCodeTrusted => true;

        public string? ProjectPath => this._roslynProject.FilePath;

        public string? TargetFramework { get; }

        public string? Configuration { get; }

        public IProjectOptions Apply( IProjectOptions options ) => throw new NotImplementedException();

        public bool TryGetProperty( string name, [NotNullWhen( true )] out string? value )
        {
            return this._properties.TryGetValue( name, out value );
        }

        public bool IsDesignTimeEnabled => false;

        public string? AdditionalCompilationOutputDirectory => null;

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
    }
}