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
        /// <c>in</c> input parameter. Synonym of <see cref="RefReadOnly"/>.
        /// </summary>
        In,

        /// <summary>
        /// <c>ref readonly</c> property or return parameter.
        /// </summary>
        RefReadOnly = In,

        /// <summary>
        /// <c>out</c>.
        /// </summary>
        Out
    }
}