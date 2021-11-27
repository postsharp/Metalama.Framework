// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Workspaces;

namespace Caravela.Framework.LinqPad
{
    public class CaravelaDataContext
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once MemberCanBePrivate.Global
#pragma warning disable SA1401
        protected readonly ProjectSet projectSet;
#pragma warning restore SA1401

        public CaravelaDataContext( string path )
        {
            this.projectSet = Workspace.Default.Load( path );
        }
    }
}