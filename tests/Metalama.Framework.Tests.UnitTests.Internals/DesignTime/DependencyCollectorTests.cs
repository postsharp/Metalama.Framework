// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Pipeline.Dependencies;
using Metalama.Framework.Engine.CodeModel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime;

public class DependencyCollectorTests : TestBase
{
    [Fact]
    public void WithinProject()
    {
        using var testContext = this.CreateTestContext();

        var code = new Dictionary<string, string>
        {
            ["Class1.cs"] = "public class Class1 { }",
            ["Class2.cs"] = "public class Class2 { }",
            ["Class3.cs"] = "public class Class3 : Class2 { }",
            ["Interface1.cs"] = "public interface Interface1 { }",
            ["Interface2.cs"] = "public interface Interface2 : Interface1 { }",
            ["Interface3.cs"] = "public interface Interface3 : Interface2 { }",
            ["Class4.cs"] = "public class Class4 : Class3, Interface3 { }"
        };

        var compilation = CreateCSharpCompilation( code );

        var dependencyCollector = new DependencyCollector( testContext.ServiceProvider, compilation, Enumerable.Empty<CompilationVersion>() );

        _ = PartialCompilation.CreatePartial( compilation, compilation.SyntaxTrees, dependencyCollector: dependencyCollector );

        var actualDependencies = string.Join(
            Environment.NewLine,
            dependencyCollector.GetDependencies().Select( x => $"'{x.MasterFilePath}'->'{x.DependentFilePath}'" ).OrderBy( x => x ) );

        var expectedDependencies = @"'Class2.cs'->'Class3.cs'
'Class3.cs'->'Class4.cs'
'Interface1.cs'->'Interface2.cs'
'Interface2.cs'->'Interface3.cs'
'Interface3.cs'->'Class4.cs'";

        Assert.Equal( expectedDependencies, actualDependencies );
    }

    [Fact]
    public void CrossProject()
    {
        using var testContext = this.CreateTestContext();

        var code1 = new Dictionary<string, string>
        {
            ["Interface1.cs"] = "public interface Interface1 { }",
            ["Interface2.cs"] = "public interface Interface2 : Interface1 { }",
            ["Interface3.cs"] = "public interface Interface3 : Interface2 { }"
        };

        var compilation1 = CreateCSharpCompilation( code1 );

        var code2 = new Dictionary<string, string>
        {
            ["Class1.cs"] = "public class Class1 { }",
            ["Class2.cs"] = "public class Class2 { }",
            ["Class3.cs"] = "public class Class3 : Class2 { }",
            ["Class4.cs"] = "public class Class4 : Class3, Interface3 { }"
        };

        var compilation2 = CreateCSharpCompilation( code2, additionalReferences: new[] { compilation1.ToMetadataReference() } );

        var dependencyCollector = new DependencyCollector(
            testContext.ServiceProvider,
            compilation2,
            new[]
            {
                new CompilationVersion(
                    compilation1,
                    compilation1.SyntaxTrees.ToImmutableDictionary( x => x.FilePath, x => new SyntaxTreeVersion( x, false, 5 ) ) )
            } );

        _ = PartialCompilation.CreatePartial( compilation2, compilation2.SyntaxTrees, dependencyCollector: dependencyCollector );

        var actualDependencies = string.Join(
            Environment.NewLine,
            dependencyCollector.GetDependencies().Select( x => $"'{x.MasterFilePath}'->'{x.DependentFilePath}'" ).OrderBy( x => x ) );

        var expectedDependencies = @"'Class2.cs'->'Class3.cs'
'Class3.cs'->'Class4.cs'
'Interface3.cs'->'Class4.cs'";

        Assert.Equal( expectedDependencies, actualDependencies );
    }
}