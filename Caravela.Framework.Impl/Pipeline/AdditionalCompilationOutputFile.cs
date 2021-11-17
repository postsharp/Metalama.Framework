// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.IO;

namespace Caravela.Framework.Impl.Pipeline
{
    public abstract class AdditionalCompilationOutputFile
    {
        public abstract AdditionalCompilationOutputFileKind Kind { get; }

        public abstract string Path { get; }

        public abstract void WriteToStream( Stream stream );

        public abstract Stream GetStream();
    }
}