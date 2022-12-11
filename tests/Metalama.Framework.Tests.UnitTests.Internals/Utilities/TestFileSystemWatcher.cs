// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Utilities;
using System.IO;

namespace Metalama.Framework.Tests.UnitTests.Utilities
{
    public sealed class TestFileSystemWatcher : IFileSystemWatcher
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