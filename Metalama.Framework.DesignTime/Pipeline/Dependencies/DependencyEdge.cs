// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

/// <summary>
/// Represents a single dependency edge between a master syntax tree and a dependent syntax tree.
/// </summary>
internal record DependencyEdge( AssemblyIdentity MasterCompilation, string MasterFilePath, ulong MasterFileHash, string DependentFilePath );