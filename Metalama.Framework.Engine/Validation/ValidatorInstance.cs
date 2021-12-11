// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.Validation;

internal class ValidatorInstance
{
    public IDeclaration ValidatedDeclaration { get; }

    public string MethodName { get; }

    public AspectPredecessor Predecessor { get; }

    public object Object => this.Predecessor.Instance;

    public IAspectState? State => (this.Object as IAspectInstance)?.State;

    public ValidatorInstance( string methodName, AspectPredecessor predecessor, IDeclaration validatedDeclaration )
    {
        this.MethodName = methodName;
        this.Predecessor = predecessor;
        this.ValidatedDeclaration = validatedDeclaration;
    }
}