// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;
using System.IO;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// Exposes build options in a strongly-typed manner. The production implementation is <see cref="BuildOptions"/>
    /// but tests can provide their own implementation.
    /// </summary>
    public interface IBuildOptions
    {
        /// <summary>
        /// Gets a value indicating whether the debugger should be attached to the process at compile time.
        /// </summary>
        bool CompileTimeAttachDebugger { get; }

        /// <summary>
        /// Gets a value indicating whether the debugger should be attached to the process at design time.
        /// </summary>
        bool DesignTimeAttachDebugger { get; }

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
        /// in the temporary directory. Use <see cref="BuildOptionsExtensions.GetCrashReportDirectoryOrDefault"/>
        /// to get a non-null value.
        /// </summary>
        string? CrashReportDirectory { get; }

        string CacheDirectory { get; }

        string ProjectId { get; }

        ImmutableArray<object> PlugIns { get; }
    }

    internal static class BuildOptionsExtensions
    {
        public static string GetCrashReportDirectoryOrDefault( this IBuildOptions options )
            => options.CrashReportDirectory ?? Path.Combine( Path.GetTempPath(), "Caravela", "Crashes" );
    }
}