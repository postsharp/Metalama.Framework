// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.Pipeline
{
    public interface ICompileTimeExceptionHandler : IService
    {
        void ReportException( Exception exception, Action<Diagnostic> reportDiagnostic, out bool mustRethrow );
    }
}