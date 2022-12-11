// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Utilities;
using Metalama.Framework.Engine;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Tests.UnitTests.Utilities
{
    internal sealed class TestFileSystemWatcherFactory : IFileSystemWatcherFactory
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