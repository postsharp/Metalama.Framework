// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Utilities
{
    internal class FileSystemWatcherFactory : IFileSystemWatcherFactory
    {
        public IFileSystemWatcher Create() => new FileSystemWatcherEx();

        public IFileSystemWatcher Create( string path ) => new FileSystemWatcherEx( path );

        public IFileSystemWatcher Create( string path, string filter ) => new FileSystemWatcherEx( path, filter );
    }
}