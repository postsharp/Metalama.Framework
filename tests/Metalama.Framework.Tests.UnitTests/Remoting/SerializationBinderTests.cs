// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.DesignTime.VisualStudio.Remoting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Remoting;

public sealed class SerializationBinderTests
{
    [Theory]
    [InlineData( typeof(int) )]
    [InlineData( typeof(ProjectKey) )]
    [InlineData( typeof(ImmutableArray<string>) )]
    [InlineData( typeof(ImmutableArray<ProjectKey>) )]
    public void Binder( Type type )
    {
        var binder = JsonSerializationBinderFactory.Instance;
        binder.BindToName( type, out var assemblyName, out var typeName );
        assemblyName = JsonSerializationBinder.RemoveAssemblyDetailsFromAssemblyName( assemblyName! );
        typeName = JsonSerializationBinder.RemoveAssemblyDetailsFromTypeName( typeName! );
        var roundloopType = binder.BindToType( assemblyName, typeName );
        Assert.Same( type, roundloopType );
    }

    [Theory]
    [InlineData(
        typeof(ImmutableArray<ProjectKey>),
        "System.Collections.Immutable.ImmutableArray`1[[Metalama.Framework.DesignTime.Rpc.ProjectKey, Metalama.Framework.DesignTime.Rpc, VERSION]]" )]
    [InlineData(
        typeof(ImmutableDictionary<string, string>),
        "System.Collections.Immutable.ImmutableDictionary`2[[System.String, System.Private.CoreLib, VERSION],[System.String, System.Private.CoreLib, VERSION]]" )]
    public void QualifyTypeName( Type type, string expectedQualifiedName )
    {
        var binder = JsonSerializationBinderFactory.Instance;
        binder.BindToName( type, out _, out var typeName );
        typeName = JsonSerializationBinder.RemoveAssemblyDetailsFromTypeName( typeName! );

        var qualified = JsonSerializationBinder.QualifyAssemblies(
            typeName,
            new Dictionary<string, string>()
            {
                { "Metalama.Framework.DesignTime.Rpc", "Metalama.Framework.DesignTime.Rpc, VERSION" },
                { "System.Collections.Immutable", "System.Collections.Immutable, VERSION" },
                { "System.Private.CoreLib", "System.Private.CoreLib, VERSION" },
                { "mscorlib", "System.Private.CoreLib, VERSION" }
            } );

        Assert.Equal( expectedQualifiedName, qualified );
    }
}