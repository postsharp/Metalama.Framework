// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Validation;
using System.Reflection;

namespace Metalama.Framework.Engine.Validation;

internal interface IValidatorDriverFactory
{
    ReferenceValidatorDriver GetReferenceValidatorDriver( MethodInfo validateMethod );

    DeclarationValidatorDriver GetDeclarationValidatorDriver( ValidatorDelegate<DeclarationValidationContext> validate );
}