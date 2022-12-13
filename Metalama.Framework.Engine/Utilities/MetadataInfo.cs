﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Utilities;

internal sealed record MetadataInfo(
    DateTime LastFileWrite,
    ImmutableDictionary<string, byte[]> Resources,
    bool HasCompileTimeAttribute,
    ImmutableDictionary<string, ImmutableArray<string>> ExportedTypes );