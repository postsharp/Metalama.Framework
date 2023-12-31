// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Services;

// ReSharper disable UnusedMember.Global

namespace Metalama.Framework.DesignTime.Utilities
{
    internal interface IFileSystemWatcherFactory : IProjectService
    {
        IFileSystemWatcher Create();

        IFileSystemWatcher Create( string path );

        IFileSystemWatcher Create( string path, string filter );
    }
}