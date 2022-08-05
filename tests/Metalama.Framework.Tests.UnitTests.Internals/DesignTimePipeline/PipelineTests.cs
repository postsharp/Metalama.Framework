// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.TestFramework;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTimePipeline
{
    public class PipelineTests : TestBase
    {
        [Fact]
        public void WithoutAspect()
        {
            using var testContext = this.CreateTestContext();

            var code = new Dictionary<string, string> { ["Class1.cs"] = "public class Class1 { }", ["Class2.cs"] = "public class Class2 { }" };

            var compilation = CreateCSharpCompilation( code );

            using var domain = new UnloadableCompileTimeDomain();
            using DesignTimeAspectPipeline pipeline = new( testContext.ServiceProvider, domain, compilation, true );
            Assert.True( pipeline.TryExecute( compilation, CancellationToken.None, out _ ) );
        }

        [Fact]
        public void WithAspect()
        {
            using var testContext = this.CreateTestContext();

            // Test that we can initialize the pipeline with a different compilation than the one with which we execute it.

            var code = new Dictionary<string, string>
            {
                ["Aspect.cs"] =
                    "public class Aspect : Metalama.Framework.Aspects.OverrideMethodAspect { public override dynamic OverrideMethod() { return null; } }",
                ["Class1.cs"] = "public class Class1 { }",
                ["Class2.cs"] = "public class Class2 { [Aspect]  void Method() {} }"
            };

            var compilation = CreateCSharpCompilation( code );

            using var domain = new UnloadableCompileTimeDomain();
            using DesignTimeAspectPipeline pipeline = new( testContext.ServiceProvider, domain, compilation, true );
            Assert.True( pipeline.TryExecute( compilation, CancellationToken.None, out _ ) );
        }
    }
}