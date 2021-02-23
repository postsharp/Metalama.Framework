// unset

namespace Caravela.Framework.Impl.Pipeline
{
    public interface IBuildOptions
    {
        /// <summary>
        /// Gets a value indicating whether the debugger should be attached to the process.
        /// </summary>
        bool AttachDebugger { get; }

        /// <summary>
        /// Gets a value indicating whether the produced PDB file should map to transformed code. When <c>false</c>, it will
        /// map to the source code.
        /// </summary>
        bool MapPdbToTransformedCode { get; }

        /// <summary>
        /// Gets the directory in which the code for the compile-time assembly should be stored, or a null or empty
        /// string to mean that the generated code should not be stored.
        /// </summary>
        string? CompileTimeProjectDirectory { get; }

        /// <summary>
        /// Gets the directory in which crash reports are stored, or a null or empty string to store
        /// in the temporary directory.
        /// </summary>
        string? CrashReportDirectory { get; }
    }
}