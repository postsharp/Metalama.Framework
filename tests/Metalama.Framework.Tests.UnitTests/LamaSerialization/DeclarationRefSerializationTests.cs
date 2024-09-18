// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.LamaSerialization;

public class DeclarationRefSerializationTests : SerializationTestsBase
{
    [Fact]
    public void ValueTypedRef()
    {
        const string code = "public class C;";
        using var testContext = this.CreateTestContext( code );
        var initialRef = testContext.Compilation.Types.Single().ToRef();

        var roundtripRef = this.TestSerialization( initialRef, testEquality: false );

        var initialSymbol = initialRef.GetTarget( testContext.Compilation );
        var roundtripSymbol = roundtripRef.GetTarget( testContext.Compilation );

        Assert.Same( initialSymbol, roundtripSymbol );
    }

    [Fact]
    public void BoxedRef()
    {
        const string code = "public class C;";
        using var testContext = this.CreateTestContext( code );
        var initialRef = testContext.Compilation.Types.Single().ToRef();

        var roundtripRef = this.TestSerialization( initialRef, testEquality: false );

        var initialSymbol = initialRef.GetTarget( testContext.Compilation );
        var roundtripSymbol = roundtripRef.GetTarget( testContext.Compilation );

        Assert.Same( initialSymbol, roundtripSymbol );
    }
}