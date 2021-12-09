// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Workspaces;
using System.Linq;

namespace Metalama.LinqPad
{
    /// <summary>
    /// Provides a <see cref="ToDump"/> method, which can be used to format object trees in the way that <see cref="MetalamaDriver"/> does,
    /// but without using <see cref="MetalamaDriver"/>.
    /// </summary>
    public static class MetalamaDumper
    {
        private static readonly FacadeObjectFactory _facadeObjectFactory = new( GetWorkspaceExpression );

        /// <summary>
        /// Formats object trees in the way that <see cref="MetalamaDriver"/> does but without using <see cref="MetalamaDriver"/>.
        /// </summary>
        public static object? ToDump( object? obj ) => _facadeObjectFactory.GetFacade( obj ) ?? obj;

        /// <summary>
        /// Generates a C# expression that can load the workspace that contains a declaration. We assume the workspace
        /// is a part of the default <see cref="WorkspaceCollection"/>.
        /// </summary>
        private static string GetWorkspaceExpression( IDeclaration declaration )
        {
            if ( !WorkspaceCollection.Default.TryFindProject( declaration.Compilation.GetRoslynCompilation(), out var workspace, out _ ) )
            {
                return "workspace";
            }

            var workspaceLoadInfo = (IWorkspaceLoadInfo) workspace;
            var arguments = string.Join( ", ", workspaceLoadInfo.LoadedPaths.Select( x => "@\"" + x + "\"" ) );

            // TODO: properties

            return $"WorkspaceCollection.Default.Load({arguments})";
        }
    }
}