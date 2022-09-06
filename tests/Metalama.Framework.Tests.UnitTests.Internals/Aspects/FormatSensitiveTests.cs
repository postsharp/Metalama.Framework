// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Aspects;

public class FormatSensitiveTests : AspectTestBase
{
    [Fact]
    public async Task CompileTimeSingleStatementUnderRunTimeIfAsync()
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

        var result = await this.CompileAsync( code );

        var transformedProperty = result!.ResultingCompilation.SyntaxTrees.Select( x => x.Value.GetRoot() )
            .SelectMany( x => x.DescendantNodes() )
            .Single( x => x is PropertyDeclarationSyntax { Identifier: { Text: "Prop132" } } )
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