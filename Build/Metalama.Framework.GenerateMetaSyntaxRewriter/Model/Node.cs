// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

#nullable disable

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Metalama.Framework.GenerateMetaSyntaxRewriter.Model
{
    public class Node : TreeType
    {
        [XmlAttribute]
        public string Root { get; set; }

        [XmlAttribute]
        public string Errors { get; set; }

        [XmlElement( ElementName = "Kind", Type = typeof(Kind) )]
        public List<Kind> Kinds { get; set; } = new();

        public List<Field> Fields { get; } = new();
    }
}