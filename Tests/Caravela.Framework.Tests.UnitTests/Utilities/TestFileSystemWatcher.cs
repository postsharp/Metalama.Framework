// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Utilities;
using System.IO;

namespace Caravela.Framework.Tests.UnitTests.Utilities
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