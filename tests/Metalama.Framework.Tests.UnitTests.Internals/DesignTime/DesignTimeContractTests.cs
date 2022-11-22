// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

#if NETFRAMEWORK
using Metalama.Framework.DesignTime.Contracts.EntryPoint;
using Metalama.Framework.Engine.Testing;
using System.IO;
using System.Reflection;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public class DesignTimeContractTests : TestBase
{
    private static readonly Assembly _loadFileAssembly = Assembly.Load( File.ReadAllBytes( typeof(DesignTimeEntryPointManager).Assembly.Location ) );

    [Fact]
    public void TypesAreEquivalent()
    {
        var mainAssembly = typeof(DesignTimeEntryPointManager).Assembly;

        Assert.NotSame( mainAssembly, _loadFileAssembly );

        foreach ( var type in _loadFileAssembly.GetTypes() )
        {
            if ( (type.IsInterface || type.IsValueType) && !type.Namespace!.StartsWith( "System" ) )
            {
                var otherType = mainAssembly.GetType( type.FullName! );
                Assert.NotSame( type, otherType );
                Assert.True( type.IsEquivalentTo( otherType ), $"The type equivalence for '{type}' is broken." );
            }
        }
    }
}
#endif