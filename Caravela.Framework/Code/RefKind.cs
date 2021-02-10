namespace Caravela.Framework.Code
{
    public enum RefKind
    {
        None,

        /// <summary>
        /// <c>ref</c>.
        /// </summary>
        Ref,

        /// <summary>
        /// <c>in</c> input parameter. Synonym of <see cref="RefReadonly"/>.
        /// </summary>
        In,

        /// <summary>
        /// <c>ref readonly</c> property or return parameter.
        /// </summary>
        RefReadonly = In,

        /// <summary>
        /// <c>out</c>.
        /// </summary>
        Out
    }
}