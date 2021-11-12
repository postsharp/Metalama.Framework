// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Pipeline;
using System.Collections.Immutable;

namespace Caravela.Framework.Tests.Integration.Runners
{
    internal partial class DesignTimeFallbackTestRunner
    {
        private class TestAuxiliaryFileProvider : IAuxiliaryFileProvider
        {
            private readonly ImmutableArray<AuxiliaryFile> _files;

            public TestAuxiliaryFileProvider( ImmutableArray<AuxiliaryFile> files )
            {
                this._files = files;
            }

            public ImmutableArray<AuxiliaryFile> GetAuxiliaryFiles()
            {
                return this._files;
            }
        }
    }
}