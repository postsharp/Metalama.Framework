// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Reflection;

namespace Metalama.Framework.DesignTime.Utilities
{
    [Obfuscation( Exclude = true )]
    internal interface IFileSystemWatcher : IDisposable
    {
        bool EnableRaisingEvents { get; set; }

        event FileSystemEventHandler Changed;

        string Path { get; set; }

        string Filter { get; set; }

        public bool IncludeSubdirectories { get; set; }
    }
}