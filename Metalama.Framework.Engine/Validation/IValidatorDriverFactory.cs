namespace Metalama.Framework.Engine.Validation;

internal interface IValidatorDriverFactory
{
    ValidatorDriver GetValidatorDriver( string name, ValidatorKind kind );
}