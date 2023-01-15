// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using LINQPad.Extensibility.DataContext;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Workspaces;
using Metalama.Testing.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.LinqPad.Tests;

#pragma warning disable VSTHRD200

public sealed class SchemaTests : UnitTestClass
{
    private readonly ITestOutputHelper _logger;

    public SchemaTests( ITestOutputHelper logger )
    {
        this._logger = logger;
    }

    [Fact]
    public void SchemaWithoutWorkspace()
    {
        var factory = new SchemaFactory( ( type, _ ) => type.ToString() );

        var schema = factory.GetSchema();

        var xml = new XDocument();
        xml.Add( new XElement( "schema", schema.SelectAsImmutableArray( item => (object) ConvertToXml( item ) ) ) );

        var xmlString = xml.ToString();
        this._logger.WriteLine( xmlString );

        // String properties should not have child properties.
        var flatSchema = schema.SelectManyRecursive( x => x.Children ?? Enumerable.Empty<ExplorerItem>() );
        var stringItem = flatSchema.First( i => i.DragText == "workspace.SourceCode.TargetFrameworks" );
        Assert.Empty( stringItem.Children ?? new List<ExplorerItem>() );
    }

    [Fact]
    public async Task SchemaWithWorkspace()
    {
        using var testContext = this.CreateTestContext();

        var projectPath = Path.Combine( testContext.ProjectOptions.BaseDirectory, "Project.csproj" );
        var codePath = Path.Combine( testContext.ProjectOptions.BaseDirectory, "Code.cs" );

        await File.WriteAllTextAsync(
            projectPath,
            @"
<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFrameworks>netstandard2.0;net6.0</TargetFrameworks>
    </PropertyGroup>
</Project>
" );

        await File.WriteAllTextAsync( codePath, "class MyClass {}" );

        var workspaceCollection = new WorkspaceCollection();

        using var workspace = await workspaceCollection.LoadAsync( projectPath );

        var factory = new SchemaFactory( ( type, _ ) => type.ToString() );

        var schema = factory.GetSchema( workspace );
        var xml = new XDocument();
        xml.Add( new XElement( "schema", schema.SelectAsImmutableArray( item => (object) ConvertToXml( item ) ) ) );
        var xmlString = xml.ToString();
        this._logger.WriteLine( xmlString );
    }

    private static XElement ConvertToXml( ExplorerItem item )
    {
        var element = new XElement(
            "item",
            new XAttribute( "text", item.Text ),
            new XAttribute( "dragText", item.DragText ?? "" ),
            new XAttribute( "tooltip", item.ToolTipText ?? "" ),
            new XAttribute( "isEnumerable", item.IsEnumerable ) );

        if ( item.Children != null )
        {
            element.Add( item.Children.SelectAsImmutableArray( explorerItem => (object) ConvertToXml( explorerItem ) ) );
        }

        return element;
    }
}