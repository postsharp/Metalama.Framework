// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;

namespace Metalama.Framework.Engine.Options
{
    public interface IPathOptions : IService
    {
        /// <summary>
        /// Returns a new path where a crash report file can be created, or <c>null</c> if no crash report can be created.
        /// </summary>
        string? GetNewCrashReportPath();

        string CompileTimeProjectCacheDirectory { get; }

        string AssemblyLocatorCacheDirectory { get; }

        string SettingsDirectory { get; }
    }
}