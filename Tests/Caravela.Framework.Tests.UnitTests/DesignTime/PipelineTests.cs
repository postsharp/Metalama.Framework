// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.DesignTime.Pipeline;
using Caravela.TestFramework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.DesignTime
{
    public class PipelineTests : TestBase
    {
        [Fact]
        public void NoAspect()
        {
            // Test that we can initialize the pipeline with a different compilation than the one with which we execute it.

            var code = new Dictionary<string, string> { ["Class1.cs"] = "public class Class1 { }", ["Class2.cs"] = "public class Class2 { }" };

            var compilation = CreateCSharpCompilation( code );

            using var buildOptions = new TestProjectOptions();
            using var domain = new UnloadableCompileTimeDomain();
            DesignTimeAspectPipeline pipeline = new( buildOptions, domain );
            var syntaxTree1 = compilation.SyntaxTrees.Single( t => t.FilePath == "Class1.cs" );
            pipeline.Execute( PartialCompilation.CreatePartial( compilation, syntaxTree1 ), CancellationToken.None );

            var syntaxTree2 = compilation.SyntaxTrees.Single( t => t.FilePath == "Class2.cs" );
            pipeline.Execute( PartialCompilation.CreatePartial( compilation, syntaxTree2 ), CancellationToken.None );
        }

        [Fact]
        public void InitializePipelineWithDifferentCompilation()
        {
            // Test that we can initialize the pipeline with a different compilation than the one with which we execute it.

            var code = new Dictionary<string, string>
            {
                ["Aspect.cs"] =
                    "public class Aspect : Caravela.Framework.Aspects.OverrideMethodAspect { public override dynamic OverrideMethod() { return null; } }",
                ["Class1.cs"] = "public class Class1 { }",
                ["Class2.cs"] = "public class Class2 { [Aspect]  void Method() {} }"
            };

            var compilation = CreateCSharpCompilation( code );

            using var buildOptions = new TestProjectOptions();
            using var domain = new UnloadableCompileTimeDomain();
            DesignTimeAspectPipeline pipeline = new( buildOptions, domain );
            var syntaxTree1 = compilation.SyntaxTrees.Single( t => t.FilePath == "Class1.cs" );
            pipeline.Execute( PartialCompilation.CreatePartial( compilation, syntaxTree1 ), CancellationToken.None );

            var syntaxTree2 = compilation.SyntaxTrees.Single( t => t.FilePath == "Class2.cs" );
            pipeline.Execute( PartialCompilation.CreatePartial( compilation, syntaxTree2 ), CancellationToken.None );
        }
    }
}