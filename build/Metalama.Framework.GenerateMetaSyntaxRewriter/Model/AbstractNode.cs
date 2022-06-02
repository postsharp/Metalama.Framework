// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

#nullable disable

using System.Collections.Generic;

namespace Metalama.Framework.GenerateMetaSyntaxRewriter.Model
{
    public class AbstractNode : TreeType
    {
        public List<Field> Fields { get; set; } = new();
    }
}