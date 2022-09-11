// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Framework.Project;

/// <summary>
/// Exception thrown when some aspect code tries to access a part of the code model that is is not allowed to access.
/// </summary>
/// <seealso cref="IExecutionContext.WithoutDependencyCollection"/>
public sealed class DeclarationOutOfScopeException : Exception
{
    public DeclarationOutOfScopeException( string message ) : base( message ) { }
}