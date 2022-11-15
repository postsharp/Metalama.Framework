// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using LINQPad.Extensibility.DataContext;
using Metalama.Framework.Code.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Metalama.LinqPad.Tests;

public class SchemaTests
{
    private readonly ITestOutputHelper _logger;

    public SchemaTests( ITestOutputHelper logger )
    {
        this._logger = logger;
    }

    [Fact]
    public void Schema()
    {
        var factory = new SchemaFactory( ( type, _ ) => type.ToString() );

        var schema = factory.GetSchema();

        var xml = new XDocument();
        xml.Add( new XElement( "schema", schema.Select( ConvertToXml ) ) );

        var xmlString = xml.ToString();
        this._logger.WriteLine( xmlString );

        // String properties should not have child properties.
        var flatSchema = schema.SelectManyRecursive( x => x.Children ?? Enumerable.Empty<ExplorerItem>() );
        var stringItem = flatSchema.First( i => i.DragText == "workspace.SourceCode.TargetFrameworks" );
        Assert.Empty( stringItem.Children ?? new List<ExplorerItem>() );

        // 'Projects' node should contain nodes for source code and transformed code.
        var projectsSchema = flatSchema.First( i => i.DragText == "workspace.Projects" );
        Assert.Contains( projectsSchema.Children, c => c.Text.StartsWith( "SourceCode" ) );
        Assert.Contains( projectsSchema.Children, c => c.Text.StartsWith( "TransformedCode" ) );
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
            element.Add( item.Children.Select( ConvertToXml ) );
        }

        return element;
    }
}