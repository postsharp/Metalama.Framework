// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Engine.Validation;

internal abstract class ValidatorDriver
{
    public Type ImplementationType { get; }

    public string MethodName { get; }

    protected ValidatorDriver( Type implementationType, string methodName )
    {
        this.ImplementationType = implementationType;
        this.MethodName = methodName;
    }
}