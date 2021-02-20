namespace Caravela.Framework.Impl.Pipeline
{
    public interface IBuildOptions
    {
        bool AttachDebugger { get; }
        bool MapPdbToTransformedCode { get; }
         
        /// <summary>
        /// Gets the directory in which the code for the compile-time assembly should be stored, or a null or empty
        /// string to mean that the generated code should not be stored.
        /// </summary>
        string? CompileTimeProjectDirectory { get; }
         
    }
}