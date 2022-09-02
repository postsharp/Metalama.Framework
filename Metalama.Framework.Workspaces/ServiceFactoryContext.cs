// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Workspaces
{
    /// <summary>
    /// The context object passed to service factories registered by <see cref="WorkspaceCollection.RegisterService"/>.
    /// </summary>
    public sealed class ServiceFactoryContext
    {
        /// <summary>
        /// Gets the MSBuild project.
        /// </summary>
        public Microsoft.Build.Evaluation.Project Project { get; }

        /// <summary>
        /// Gets the Roslyn compilation.
        /// </summary>
        public Compilation Compilation { get; }

        /// <summary>
        /// Gets the target framework.
        /// </summary>
        public string? TargetFramework { get; }

        internal ServiceFactoryContext( Microsoft.Build.Evaluation.Project project, Compilation compilation, string? targetFramework )
        {
            this.Project = project;
            this.Compilation = compilation;
            this.TargetFramework = targetFramework;
        }
    }
}