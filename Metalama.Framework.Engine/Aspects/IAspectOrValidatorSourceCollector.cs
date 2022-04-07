using Metalama.Framework.Engine.Validation;

namespace Metalama.Framework.Engine.Aspects;

internal interface IAspectOrValidatorSourceCollector
{
    void AddAspectSource( IAspectSource aspectSource );

    void AddValidatorSource( IValidatorSource validatorSource );
}