// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.IO;

namespace Caravela.Framework.Impl.Pipeline
{
    internal class GeneratedAdditionalCompilationOutputFile : AdditionalCompilationOutputFile
    {
        public override string Path { get; }

        public override AdditionalCompilationOutputFileKind Kind { get; }

        private readonly Action<Stream> _writeAction;

        public GeneratedAdditionalCompilationOutputFile( string path, AdditionalCompilationOutputFileKind kind, Action<Stream> writeAction )
        {
            this.Path = path;
            this.Kind = kind;
            this._writeAction = writeAction;
        }

        public override Stream GetStream()
        {
            throw new NotSupportedException();
        }

        public override void WriteToStream( Stream stream )
        {
            this._writeAction( stream );
        }
    }
}