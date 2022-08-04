// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

/// <summary>
/// Represents a single dependency edge between a master syntax tree and a dependent syntax tree. This object is used for test only.
/// </summary>
internal record SyntaxTreeDependency( string MasterFilePath, string DependentFilePath );