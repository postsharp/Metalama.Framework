using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Validation;

namespace Metalama.Framework.Engine.Fabrics;

internal interface IDeclarationSelectorInternal : IValidatorDriverFactory
{
    AspectPredecessor AspectPredecessor { get; }
}