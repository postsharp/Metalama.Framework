// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.IO;

namespace Metalama.Framework.Engine.Utilities.Diagnostics
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

        public override string ToString() => $"{this.ProcessName}({this.ProcessId})";
    }
}