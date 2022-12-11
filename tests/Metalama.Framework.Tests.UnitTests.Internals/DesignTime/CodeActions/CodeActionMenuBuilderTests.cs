// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.CodeFixes;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime.CodeActions;

public sealed class CodeActionMenuBuilderTests
{
    [Fact]
    public void TopLevelItem()
    {
        var builder = new CodeActionMenuBuilder();

        // Space in the string intentional to test trimming.
        builder.AddItem( "TopLevel ", t => new UserCodeActionModel { Title = t } );
        var items = builder.Build();

        Assert.Single( items );
        Assert.IsAssignableFrom<UserCodeActionModel>( items[0] );
        Assert.Equal( "TopLevel", items[0].Title );
    }

    [Fact]
    public void TwoLevels()
    {
        var builder = new CodeActionMenuBuilder();

        // Space in the string intentional to test trimming.
        builder.AddItem( "level 1 | level 2 | level 3", t => new UserCodeActionModel { Title = t } );
        var items = builder.Build();

        var level1 = Assert.IsAssignableFrom<CodeActionMenuModel>( Assert.Single( items ) );
        Assert.Equal( "level 1", level1.Title );
        var level2 = Assert.IsAssignableFrom<CodeActionMenuModel>( Assert.Single( level1.Items ) );
        Assert.Equal( "level 2", level2.Title );
        var level3 = Assert.IsAssignableFrom<UserCodeActionModel>( Assert.Single( level2.Items ) );

        Assert.Equal( "level 3", level3.Title );
    }
}