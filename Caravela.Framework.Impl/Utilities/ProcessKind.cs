// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Utilities
{
    internal enum ProcessKind
    {
        Other,
        Compiler,
        DevEnv,
        RoslynCodeAnalysisService,
        Rider
    }
}