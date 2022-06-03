// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Xml;
using System.Xml.Serialization;

namespace Metalama.Framework.GenerateMetaSyntaxRewriter.Model;

public static class TreeReader
{
    public static Tree ReadTree( string inputFile )
    {
        var reader = XmlReader.Create( inputFile, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit } );
        var serializer = new XmlSerializer( typeof(Tree) );
        var tree = (Tree) serializer.Deserialize( reader );
        TreeFlattening.FlattenChildren( tree );

        return tree;
    }
}