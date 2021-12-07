// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;

namespace Metalama.Framework.Impl.Utilities
{
    internal interface IFileSystemWatcherFactory : IService
    {
        IFileSystemWatcher Create();

        IFileSystemWatcher Create( string path );

        IFileSystemWatcher Create( string path, string filter );
    }
}