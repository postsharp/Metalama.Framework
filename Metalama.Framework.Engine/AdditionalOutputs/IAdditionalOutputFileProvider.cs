// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;
using System.Collections.Immutable;

namespace Metalama.Framework.Impl.AdditionalOutputs
{
    public interface IAdditionalOutputFileProvider : IService
    {
        ImmutableArray<AdditionalCompilationOutputFile> GetAdditionalCompilationOutputFiles();
    }
}