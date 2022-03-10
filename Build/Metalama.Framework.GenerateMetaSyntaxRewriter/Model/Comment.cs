// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

#nullable disable

using System.Xml;
using System.Xml.Serialization;

namespace Metalama.Framework.GenerateMetaSyntaxRewriter.Model
{
    public class Comment
    {
        [XmlAnyElement]
        public XmlElement[] Body { get; set; }
    }
}