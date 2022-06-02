// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Metalama.Framework.GenerateMetaSyntaxRewriter;

/// <summary>
/// Cleans the Syntax-*.xml files so that they can be easily compared.
/// </summary>
internal static class SyntaxXmlCleaner
{
    public static void Clean( string fileName )
    {
        var document = XDocument.Load( fileName );

        var hasChange = false;

        var nodesToRemove = document.Root!.XPathSelectElements(
                "//TypeComment | //PropertyComment | //FactoryComment | //summary",
                new XmlNamespaceManager( new NameTable() ) )
            .ToList();

        foreach ( var element in nodesToRemove )
        {
            hasChange = true;
            element.Remove();
        }

        if ( hasChange )
        {
            document.Save( fileName );
        }
    }
}