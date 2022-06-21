// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Utilities;

internal record MetadataInfo( DateTime LastFileWrite, ImmutableDictionary<string, byte[]> Resources, bool HasCompileTimeAttribute );