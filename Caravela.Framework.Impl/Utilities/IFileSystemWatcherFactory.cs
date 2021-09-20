// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Utilities
{
    internal interface IFileSystemWatcherFactory
    {
        IFileSystemWatcher Create();
        
        IFileSystemWatcher Create( string path );
        
        IFileSystemWatcher Create( string path, string filter );
    }
}