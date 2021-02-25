namespace Caravela.Framework.Code
{
    public interface IParameterBuilder : IParameter, ICodeElementBuilder
    {

        /// <remarks>
        /// Gets or sets the default value of the parameter, or  <c>default</c> if the parameter type is a struct and the default
        /// value of the parameter is the default value of the struct type.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">The parameter has no default value.</exception>
        new OptionalValue DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the parameter type.
        /// </summary>
        new IType ParameterType { get; set; }
    }
}