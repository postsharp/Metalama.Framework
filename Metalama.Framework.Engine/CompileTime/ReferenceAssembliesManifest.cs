﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CompileTime;

internal sealed record ReferenceAssembliesManifest( ImmutableArray<string> ReferenceAssemblies, ImmutableDictionary<string, ImmutableHashSet<string>> Types );