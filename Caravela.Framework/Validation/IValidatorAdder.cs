using Caravela.Framework.Code;
using System;
using System.Collections.Generic;

namespace Caravela.Framework.Validation
{
    public interface IValidatorAdder
    {
        void RegisterTargetValidator<TTarget>( TTarget targetDeclaration, Action<ValidateReferenceContext<TTarget>> validator )
            where TTarget : IDeclaration;
        
        // The reason why the user must pass a type name with string parameters is that we want to prevent the user to pass state of the current
        // compilation to the validator. The validator must be stateless; all state just be serialized as a strings. This restriction is important
        // at design time but not at compile time.
        void RegisterReferenceValidator<TTarget, TConstraint>( TTarget targetDeclaration, IReadOnlyList<DeclarationReferenceKind> referenceKinds, IReadOnlyDictionary<string, string>? properties = null )
            where TTarget : IDeclaration
            where TConstraint : IDeclarationReferenceValidator<TTarget>, new();
    }
}