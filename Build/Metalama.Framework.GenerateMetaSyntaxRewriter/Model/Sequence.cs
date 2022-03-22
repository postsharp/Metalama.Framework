// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Metalama.Framework.GenerateMetaSyntaxRewriter.Model;

public class Sequence : TreeTypeChild
{
    // Note: 'Sequence's should not be children of a 'Sequence'.  It's not necessary, and the
    // child choice can just be inlined into the parent.
    [XmlElement( ElementName = "Field", Type = typeof(Field) )]
    [XmlElement( ElementName = "Choice", Type = typeof(Choice) )]
    public List<TreeTypeChild> Children { get; set; } = new();
}