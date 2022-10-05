// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Immutable;
using System.Reflection;

namespace Metalama.Framework.Engine.CompileTime;

[Obfuscation( Exclude = true /* Json */ )]
internal record ReferenceAssembliesManifest( ImmutableArray<string> Assemblies, ImmutableDictionary<string, ImmutableHashSet<string>> Types );