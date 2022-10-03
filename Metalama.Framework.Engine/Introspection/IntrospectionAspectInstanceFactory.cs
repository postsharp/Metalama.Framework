// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Project;
using System.Collections.Concurrent;

namespace Metalama.Framework.Engine.Introspection;

internal class IntrospectionAspectInstanceFactory : IService
{
    private readonly ConcurrentDictionary<IAspectInstance, IntrospectionAspectInstance> _instances = new();
    private readonly CompilationModel _compilation;

    public IntrospectionAspectInstanceFactory( CompilationModel compilation )
    {
        this._compilation = compilation;
    }

    public IntrospectionAspectInstance GetIntrospectionAspectInstance( IAspectInstance aspectInstance )
        => this._instances.GetOrAdd( aspectInstance, x => new IntrospectionAspectInstance( x, this._compilation, this ) );
}