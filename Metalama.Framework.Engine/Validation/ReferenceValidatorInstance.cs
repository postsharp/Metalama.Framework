using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Engine.Validation;

internal class ReferenceValidatorInstance : ValidatorInstance
{
    public ReferenceValidatorInstance(
        AspectPredecessor aspectPredecessor,
        IDeclaration declaration,
        string methodName,
        ValidatedReferenceKinds referenceKinds ) : base( methodName, aspectPredecessor, declaration)
    {
        this.ReferenceKinds = referenceKinds;
    }

    // Aspect or fabric.
    
    public ValidatedReferenceKinds ReferenceKinds { get; }
}