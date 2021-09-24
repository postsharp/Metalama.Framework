namespace Caravela.Framework.Code
{
    /// <summary>
    /// Specifies the kind variance: <see cref="In"/>, <see cref="Out"/> or <see cref="None"/>.
    /// </summary>
    public enum VarianceKind
    {
        /// <summary>
        /// No variance.
        /// </summary>
        None,
        
        /// <summary>
        /// Contravariant.
        /// </summary>
        In,
        
        /// <summary>
        /// Covariant.
        /// </summary>
        Out
    }
}