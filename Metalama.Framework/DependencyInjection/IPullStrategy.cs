// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.DependencyInjection;

[CompileTime]
public interface IPullStrategy
{
    PullAction PullFieldOrProperty( IFieldOrProperty fieldOrProperty, IConstructor constructor, ScopedDiagnosticSink diagnosticSink );

    PullAction PullParameter( IParameter parameter, IConstructor constructor, ScopedDiagnosticSink diagnosticSink );
}