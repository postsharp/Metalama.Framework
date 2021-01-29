namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents an array, e.g. <c>T[]</c>.
    /// </summary>
    public interface IArrayType : IType
    {
        /// <summary>
        /// Gets the element type, e.g. the <c>T</c> in <c>T[]</c>.
        /// </summary>
        IType ElementType { get; }
        
        /// <summary>
        /// Gets the array rank (1 for <c>T[]</c>, 2 for <c>T[,]</c>, ...).
        /// </summary>
        int Rank { get; }
    }
}