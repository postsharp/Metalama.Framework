// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Aspects;

public class FormatSensitiveTests : TestBase
{
    [Fact]
    public async Task CompileTimeSingleStatementUnderRunTimeIf()
    {
        var code = @"
using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;

public class PropOverride : OverrideFieldOrPropertyAspect
{
    public override dynamic? OverrideProperty
    {
        get
        {
            return meta.Proceed()?.ToUpper();
        }
        set
        {
            // Here we are trying different kinds of compile-time statements.
            if (1 == 1)
                meta.Proceed();

            if (1 == 1)
                _ = meta.Proceed();

            if (1 == 1)
                meta.InsertComment( ""x"" );

        if (1 == 1)
            if (meta.Target.Declaration != null)
            {
                meta.Proceed();
            }
    }
}
}

public class TestClass
{
    [PropOverride]
    public string Prop132 { get; set; }
}
";

        using var domain = new UnloadableCompileTimeDomain();
        var testContext = this.CreateTestContext();
        var compilation = CreateCSharpCompilation( code );

        var pipeline = new CompileTimeAspectPipeline( testContext.ServiceProvider, true, domain );
        var diagnostics = new DiagnosticList();
        var result = await pipeline.ExecuteAsync( diagnostics, compilation, ImmutableArray<ManagedResource>.Empty, CancellationToken.None );

        var transformedSyntaxRoot = await result!.ResultingCompilation.SyntaxTrees.Single( t => t.Key == compilation.SyntaxTrees.Single().FilePath )
            .Value.GetRootAsync();

        var transformedProperty = transformedSyntaxRoot
            .DescendantNodes()
            .Single( x => x is PropertyDeclarationSyntax property && property.Identifier.Text == "Prop132" )
            .NormalizeWhitespace()
            .ToString();

        var expectedTransformedProperty = @"
[PropOverride]
public string Prop132
{
    get
    {
        return (global::System.String)this.Prop132_Source?.ToUpper();
    }

    set
    {
        if (1 == 1)
        {
            this.Prop132_Source = value;
        }

        if (1 == 1)
            this.Prop132_Source = value;
        if (1 == 1)
        {
        // x
        }

        if (1 == 1)
        {
            this.Prop132_Source = value;
        }
    }
}";

        Assert.Equal( expectedTransformedProperty.Trim(), transformedProperty.Trim() );
    }
}