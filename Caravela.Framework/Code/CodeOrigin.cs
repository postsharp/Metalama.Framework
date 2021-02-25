namespace Caravela.Framework.Code
{
    /// <summary>
    /// Origins of an element of code.
    /// </summary>
    public enum CodeOrigin
    {
        /// <summary>
        /// Source code.
        /// </summary>
        Source,

        /// <summary>
        /// Roslyn code generator.
        /// </summary>
        Generator,

        /// <summary>
        /// Aspect (introduction).
        /// </summary>
        Aspect
    }
}