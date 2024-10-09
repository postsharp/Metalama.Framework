// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel.Helpers;
using Metalama.Testing.UnitTesting;
using Xunit;
using Xunit.Abstractions;
using static Metalama.Framework.Tests.UnitTests.Utilities.SerializableDeclarationIdTests;

namespace Metalama.Framework.Tests.UnitTests.Utilities;

public sealed class SerializableDeclarationIdForIntroducedTests : UnitTestClass
{
    public SerializableDeclarationIdForIntroducedTests( ITestOutputHelper? testOutputHelper = null ) : base( testOutputHelper, false ) { }

    [Fact]
    public void TestAllDeclarations()
    {
        const string code = """
                            using Metalama.Framework.Advising;
                            using Metalama.Framework.Aspects; 
                            """ +
#if !ROSLYN_4_4_0_OR_GREATER
                            """
                            class Aspect : TypeAspect
                            """ +
#else
                            """
                            class Aspect<T> : TypeAspect
                            """ +
#endif
                            """
                            {
                              [Introduce]
                              void M<T2>(int p) {}
                              [Introduce]
                              int this[int i] => 0;
                              [Introduce]
                              int _field;
                              [Introduce]
                              event System.EventHandler Event;
                              [Introduce]
                              ~Aspect() {}
                            }

                            """ +
#if !ROSLYN_4_4_0_OR_GREATER
                            """
                            [Aspect]
                            """ +
#else
                            """
                            [Aspect<int>]
                            """ +
#endif
                            """
                            class C { }
                            """;

        using var testContext = this.CreateTestContext();
        var compilation = testContext.CreateCompilation( code );

        foreach ( var declaration in compilation.GetContainedDeclarations() )
        {
            Roundtrip( declaration, compilation, this.TestOutput );
        }

        Roundtrip( compilation, compilation, this.TestOutput );
    }
}