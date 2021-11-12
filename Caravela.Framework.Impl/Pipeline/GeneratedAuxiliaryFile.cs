// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Pipeline
{
    internal class GeneratedAuxiliaryFile : AuxiliaryFile
    {
        public override string Path { get; }

        public override AuxiliaryFileKind Kind { get; }

        public override byte[] Content { get; }

        public GeneratedAuxiliaryFile( string path, AuxiliaryFileKind kind, byte[] content )
        {
            this.Path = path;
            this.Kind = kind;
            this.Content = content;
        }
    }
}