// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.Validation;

internal class DeclarationValidatorInstance : ValidatorInstance
{
    public DeclarationValidatorInstance( AspectPredecessor predecessor, IDeclaration validatedDeclaration, string methodName ) : base(
        methodName,
        predecessor,
        validatedDeclaration ) { }
}