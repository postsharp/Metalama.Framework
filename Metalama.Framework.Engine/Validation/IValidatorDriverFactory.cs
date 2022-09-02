// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Validation;
using System.Reflection;

namespace Metalama.Framework.Engine.Validation;

internal interface IValidatorDriverFactory
{
    ReferenceValidatorDriver GetReferenceValidatorDriver( MethodInfo validateMethod );

    DeclarationValidatorDriver GetDeclarationValidatorDriver( ValidatorDelegate<DeclarationValidationContext> validate );
}