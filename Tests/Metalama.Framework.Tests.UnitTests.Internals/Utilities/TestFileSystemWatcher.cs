// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using System.IO;

namespace Metalama.Framework.Tests.UnitTests.Utilities
{
    public class TestFileSystemWatcher : IFileSystemWatcher
    {
        public TestFileSystemWatcher( string path, string filter )
        {
            this.Path = path;
            this.Filter = filter;
        }

        public bool EnableRaisingEvents { get; set; }

        public event FileSystemEventHandler? Changed;

        public string Path { get; set; }

        public string Filter { get; set; }

        public bool IncludeSubdirectories { get; set; }

        public void Dispose() { }

        public void Notify( FileSystemEventArgs args )
        {
            this.Changed?.Invoke( this, args );
        }
    }
}