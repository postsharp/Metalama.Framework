// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Validation
{
    /// <summary>
    /// (Not implemented.)
    /// </summary>
    [CompileTimeOnly]
    public interface IValidatorAdder
    {
        [Obsolete( "Not implemented." )]
        void AddTargetValidator<TTarget>( TTarget targetDeclaration, Action<ValidateReferenceContext<TTarget>> validator )
            where TTarget : IDeclaration;

        // The reason why the user must pass a type name with string parameters is that we want to prevent the user to pass state of the current
        // compilation to the validator. The validator must be stateless; all state just be serialized as a strings. This restriction is important
        // at design time but not at compile time.

        [Obsolete( "Not implemented." )]
        void AddReferenceValidator<TTarget, TConstraint>(
            TTarget targetDeclaration,
            IReadOnlyList<DeclarationReferenceKind> referenceKinds,
            IReadOnlyDictionary<string, string>? properties = null )
            where TTarget : IDeclaration
            where TConstraint : IDeclarationReferenceValidator<TTarget>, new();
    }
}