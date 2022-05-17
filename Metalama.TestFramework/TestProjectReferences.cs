// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.TestFramework;

public record TestProjectReferences( ImmutableArray<MetadataReference> MetadataReferences, ImmutableArray<object> PlugIns, string? GlobalUsingsFile );