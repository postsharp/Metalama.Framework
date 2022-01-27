// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.DesignTime.Utilities
{
    internal class FileSystemWatcherEx : FileSystemWatcher, IFileSystemWatcher
    {
        public FileSystemWatcherEx() { }

        public FileSystemWatcherEx( string path ) : base( path ) { }

        public FileSystemWatcherEx( string path, string filter ) : base( path, filter ) { }
    }
}