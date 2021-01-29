namespace Caravela.Framework.Code
{
    /// <summary>
    /// Represents an unsafe pointer type.
    /// </summary>
    public interface IPointerType : IType
    {
        /// <summary>
        /// Gets the type pointed at, that is, <c>T</c> for <c>*T</c>.
        /// </summary>
        IType PointedAtType { get; }
    }
}