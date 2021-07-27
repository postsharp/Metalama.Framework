namespace Caravela.Framework.Code.Invokers
{
    /// <summary>
    /// Kinds of member access operator: <c>.</c> or <c>?.</c>.
    /// </summary>
    public enum InvokerOperator
    {
        /// <summary>
        /// Default '.' operator.
        /// </summary>
        Default,

        /// <summary>
        /// Conditional ('?.') operator.
        /// </summary>
        Conditional
    }
}