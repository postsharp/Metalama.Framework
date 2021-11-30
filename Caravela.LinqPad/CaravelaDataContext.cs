// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Workspaces;

namespace Caravela.LinqPad
{
    /// <summary>
    /// The base class for all queries created with <see cref="CaravelaDriver"/>.
    /// </summary>
    public class CaravelaDataContext
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBePrivate.Global
#pragma warning disable SA1401
        protected readonly Workspace workspace;
#pragma warning restore SA1401

        public CaravelaDataContext( string path )
        {
            this.workspace = WorkspaceCollection.Default.Load( path );
        }
    }
}