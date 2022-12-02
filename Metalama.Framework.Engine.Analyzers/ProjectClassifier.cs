// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Concurrent;

namespace Metalama.Framework.Engine.Analyzers
{
    internal static class ProjectClassifier
    {
        private static readonly ConcurrentDictionary<string, ProjectKind> _cache = new();

        public static ProjectKind GetProjectKind( string name ) => _cache.GetOrAdd( name, GetProjectKindCore );

        private static ProjectKind GetProjectKindCore( string name )
        {
            if ( name.StartsWith( "Metalama.Framework.Engine." ) ||
                 name.StartsWith( "Metalama.Framework.DesignTime." ) )
            {
                return ProjectKind.MetalamaInternal;
            }
            else if ( name == "Metalama.Testing.UnitTesting" || name == "Metalama.Testing.AspectTesting" || name == "Metalama.Framework.Workspaces" || name == "Metalama.LinqPad" )
            {
                return ProjectKind.MetalamaPublicApi;
            }
            else
            {
                return ProjectKind.UserProject;
            }
        }
    }
}