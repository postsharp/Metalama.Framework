// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.IO;

namespace Metalama.Framework.Engine.Utilities
{
    public sealed class ProcessInfo
    {
        public int ProcessId { get; }

        public string? ImagePath { get; }

        public string? ProcessName => this.ImagePath == null ? null : Path.GetFileNameWithoutExtension( this.ImagePath ).ToLowerInvariant();

        public ProcessInfo( int processId, string? imageFileName )
        {
            this.ProcessId = processId;
            this.ImagePath = imageFileName;
        }
    }
}