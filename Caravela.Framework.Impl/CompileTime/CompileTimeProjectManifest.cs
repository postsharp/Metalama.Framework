// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CompileTime
{
    internal class CompileTimeProjectManifest
    {
        public string? AssemblyName { get; set; }

        public Version? Version { get; set; }

        public List<string>? AspectTypes { get; set; }

        public List<string>? References { get; set; }

        public ulong Hash { get; set; }
    }
}