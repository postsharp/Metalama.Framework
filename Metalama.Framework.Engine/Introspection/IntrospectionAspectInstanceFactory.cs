// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using System.Collections.Concurrent;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionAspectInstanceFactory
{
    private readonly ConcurrentDictionary<AspectInstanceResult, IntrospectionAspectInstance> _instances = new();
    private readonly CompilationModel _compilation;

    public IntrospectionAspectInstanceFactory( CompilationModel compilation )
    {
        this._compilation = compilation;
    }

    public IntrospectionAspectInstance GetEvaluatedAspectInstance( AspectInstanceResult aspectInstanceResult )
        => this._instances.GetOrAdd( aspectInstanceResult, x => new IntrospectionAspectInstance( x, this._compilation, this ) );
}