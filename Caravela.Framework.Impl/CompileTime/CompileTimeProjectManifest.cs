// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Caravela.Framework.Impl.CompileTime
{

    internal class CompileTimeProjectManifest
    {
        public string? AssemblyName { get; set; }

        public Version? Version { get; set; }

        public List<string>? AspectTypes { get; set; }

        public List<string>? References { get; set; }
    }
}