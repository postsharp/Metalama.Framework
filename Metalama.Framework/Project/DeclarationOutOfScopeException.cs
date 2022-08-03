// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Project;

/// <summary>
/// Exception thrown when some aspect code tries to access a part of the code model that is is not allowed to access.
/// </summary>
public sealed class DeclarationOutOfScopeException : Exception
{
    public DeclarationOutOfScopeException( string message ) : base( message ) { }
}