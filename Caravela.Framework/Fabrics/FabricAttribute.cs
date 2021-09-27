// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Fabrics
{
    public sealed class FabricAttribute : Attribute
    {
        public string? TargetTypeName { get; set; }

        public string? Path { get; set; }
    }
}