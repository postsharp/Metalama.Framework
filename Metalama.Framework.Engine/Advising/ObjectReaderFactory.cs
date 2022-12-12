// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Advising;

internal class ObjectReaderFactory : IProjectService
{
    private readonly ConcurrentDictionary<Type, ObjectReaderTypeAdapter> _types = new();

    private readonly ProjectServiceProvider _serviceProvider;

    public IObjectReader GetReader( object? instance )
        => instance switch
        {
            null => ObjectReader.Empty,
            IObjectReader objectReader => objectReader,
            IReadOnlyDictionary<string, object?> dictionary => new ObjectReaderDictionaryWrapper( dictionary ),
            _ => new ObjectReader( instance, this.GetTypeAdapter( instance.GetType() ) )
        };

    private ObjectReaderTypeAdapter GetTypeAdapter( Type type )
    {
        return this._types.GetOrAdd( type, t => new ObjectReaderTypeAdapter( this._serviceProvider, t ) );
    }

    public ObjectReaderFactory( ProjectServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
    }
}