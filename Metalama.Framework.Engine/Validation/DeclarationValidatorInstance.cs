using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Engine.Validation;

internal class DeclarationValidatorInstance : ValidatorInstance
{
    public DeclarationValidatorInstance( AspectPredecessor predecessor, IDeclaration validatedDeclaration, string methodName ) : base( methodName, predecessor, validatedDeclaration )
    {
    }
}