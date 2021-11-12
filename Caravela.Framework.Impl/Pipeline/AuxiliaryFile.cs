// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Pipeline
{
    public abstract class AuxiliaryFile
    {
        public abstract AuxiliaryFileKind Kind { get; }

        public abstract string Path { get; }

        public abstract byte[] Content { get; }
    }
}