namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Something (like a parameter or a local variable) that has a value that can be read or assigned at run time.
    /// </summary>
    public interface IHasRuntimeValue
    {
        /// <summary>
        /// Gets or sets the value at run time.
        /// </summary>
        dynamic Value { get; set; }
    }
}