// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.DesignTime.Utilities
{
    internal interface IFileSystemWatcher : IDisposable
    {
        bool EnableRaisingEvents { get; set; }

        event FileSystemEventHandler Changed;

        string Path { get; }

        string Filter { get; }

        public bool IncludeSubdirectories { get; set; }
    }
}