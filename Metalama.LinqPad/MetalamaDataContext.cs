// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Introspection;

namespace Metalama.LinqPad
{
    /// <summary>
    /// The base class for all queries created with <see cref="MetalamaDriver"/>.
    /// </summary>
    public class MetalamaDataContext
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBePrivate.Global
#pragma warning disable SA1401, IDE1006
        protected readonly Workspace workspace;
#pragma warning restore SA1401, IDE1006

        public MetalamaDataContext( string path )
        {
            this.workspace = WorkspaceCollection.Default.Load( path );
        }
    }
}