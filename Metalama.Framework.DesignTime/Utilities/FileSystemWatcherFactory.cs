// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.DesignTime.Utilities
{
    internal class FileSystemWatcherFactory : IFileSystemWatcherFactory
    {
        public IFileSystemWatcher Create() => new FileSystemWatcherEx();

        public IFileSystemWatcher Create( string path ) => new FileSystemWatcherEx( path );

        public IFileSystemWatcher Create( string path, string filter ) => new FileSystemWatcherEx( path, filter );
    }
}