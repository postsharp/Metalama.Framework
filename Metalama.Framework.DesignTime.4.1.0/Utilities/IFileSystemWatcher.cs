// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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