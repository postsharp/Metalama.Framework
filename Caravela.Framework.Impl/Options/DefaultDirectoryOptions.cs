// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Utilities;

namespace Caravela.Framework.Impl.Options
{
    /// <summary>
    /// A default implementation of <see cref="IDirectoryOptions"/>.
    /// </summary>
    public class DefaultDirectoryOptions : IDirectoryOptions
    {
        protected DefaultDirectoryOptions() { }

        public static readonly DefaultDirectoryOptions Instance = new();

        public virtual string CompileTimeProjectCacheDirectory => TempPathHelper.GetTempPath( "CompileTime" );

        public virtual string AssemblyLocatorCacheDirectory => TempPathHelper.GetTempPath( "AssemblyLocator" );

        public virtual string CrashReportDirectory => TempPathHelper.GetTempPath( "CrashReports" );
    }
}