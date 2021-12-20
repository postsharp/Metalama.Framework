// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Engine.Validation;

/// <summary>
/// Validation drivers cache and execute the method call to the user validation method.
/// </summary>
internal abstract class ValidatorDriver
{
    /// <summary>
    /// Gets the type defining the validation method.
    /// </summary>
    public Type ImplementationType { get; }

    /// <summary>
    /// Gets the name of the validation method.
    /// </summary>
    public string MethodName { get; }

    protected ValidatorDriver( Type implementationType, string methodName )
    {
        this.ImplementationType = implementationType;
        this.MethodName = methodName;
    }
}