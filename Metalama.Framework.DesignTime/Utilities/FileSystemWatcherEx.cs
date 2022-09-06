// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.DesignTime.Utilities
{
    internal class FileSystemWatcherEx : FileSystemWatcher, IFileSystemWatcher
    {
        public FileSystemWatcherEx() { }

        public FileSystemWatcherEx( string path ) : base( path ) { }

        public FileSystemWatcherEx( string path, string filter ) : base( path, filter ) { }
    }
}