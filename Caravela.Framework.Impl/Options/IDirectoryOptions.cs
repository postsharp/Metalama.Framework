// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.IO;

namespace Caravela.Framework.Impl.Options
{
    public interface IDirectoryOptions
    {
        string CompileTimeProjectCacheDirectory { get; }
        
        string AssemblyLocatorCacheDirectory { get; }
        
        /// <summary>
        /// Gets the directory in which crash reports are stored, or a null or empty string to store
        /// in the temporary directory.
        /// </summary>
        string CrashReportDirectory { get; }
    }
}