// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

#nullable disable

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Metalama.Framework.GenerateMetaSyntaxRewriter.Model
{
    public class TreeType
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Base { get; set; }

        [XmlAttribute]
        public string SkipConvenienceFactories { get; set; }

        [XmlElement]
        public Comment TypeComment { get; set; }

        [XmlElement]
        public Comment FactoryComment { get; set; }

        [XmlElement( ElementName = "Field", Type = typeof(Field) )]
        [XmlElement( ElementName = "Choice", Type = typeof(Choice) )]
        [XmlElement( ElementName = "Sequence", Type = typeof(Sequence) )]
        public List<TreeTypeChild> Children { get; set; } = new();

        [XmlIgnore]
        public RoslynVersion? MinimalRoslynVersion { get; set; }
    }
}