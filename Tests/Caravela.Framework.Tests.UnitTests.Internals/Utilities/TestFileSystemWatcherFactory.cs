// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Impl.Utilities;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Tests.UnitTests.Utilities
{
    internal class TestFileSystemWatcherFactory : IFileSystemWatcherFactory
    {
        private readonly Dictionary<(string Path, string Filter), IFileSystemWatcher> _watchers = new();

        public void Add( IFileSystemWatcher watcher )
        {
            this._watchers.Add( (watcher.Path.AssertNotNull(), watcher.Filter), watcher );
        }

        public IFileSystemWatcher Create() => throw new NotImplementedException();

        public IFileSystemWatcher Create( string path ) => throw new NotImplementedException();

        public IFileSystemWatcher Create( string path, string filter ) => this._watchers[(path, filter)];

        public IFileSystemWatcher Create( string path, string filter, bool includeSubDirectories ) => this._watchers[(path, filter)];
    }
}