// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Pipeline
{
    public interface ICompileTimeExceptionHandler : IService
    {
        void ReportException( Exception exception, Action<Diagnostic> reportDiagnostic, bool canIgnoreException, out bool isHandled );
    }
}