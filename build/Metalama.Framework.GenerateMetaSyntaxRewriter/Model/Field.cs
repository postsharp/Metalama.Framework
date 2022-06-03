// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

#nullable disable

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Metalama.Framework.GenerateMetaSyntaxRewriter.Model
{
    public class Field : TreeTypeChild
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Type { get; set; }

        [XmlAttribute]
        public string Optional { get; set; }

        [XmlAttribute]
        public string Override { get; set; }

        [XmlAttribute]
        public string New { get; set; }

        [XmlAttribute]
        public int MinCount { get; set; }

        [XmlAttribute]
        public bool AllowTrailingSeparator { get; set; }

        [XmlElement( ElementName = "Kind", Type = typeof( Kind ) )]
        public List<Kind> Kinds { get; set; } = new();

        [XmlElement]
        public Comment PropertyComment { get; set; }

        public bool IsToken => this.Type == "SyntaxToken";

        public bool IsOptional => this.Optional == "true";

        [XmlIgnore]
        public RoslynVersion MinimalRoslynVersion { get; set; }

        [XmlIgnore]
        public Dictionary<Kind, RoslynVersion> KindsMinimalRoslynVersions { get; set; }
    }
}