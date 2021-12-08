// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.Utilities;
using System;
using System.IO;

namespace Metalama.Framework.Impl.Options
{
    /// <summary>
    /// A default implementation of <see cref="IPathOptions"/>.
    /// </summary>
    public class DefaultPathOptions : IPathOptions
    {
        private static readonly string _crashReportDirectory = TempPathHelper.GetTempPath( "CrashReports" );

        protected DefaultPathOptions() { }

        public static readonly DefaultPathOptions Instance = new();

        public virtual string? GetNewCrashReportPath()
        {
            RetryHelper.Retry(
                () =>
                {
                    if ( !Directory.Exists( _crashReportDirectory ) )
                    {
                        Directory.CreateDirectory( _crashReportDirectory );
                    }
                } );

            return Path.Combine( _crashReportDirectory, $"exception-{Guid.NewGuid()}.log" );
        }

        public virtual string CompileTimeProjectCacheDirectory => TempPathHelper.GetTempPath( "CompileTime" );

        public virtual string AssemblyLocatorCacheDirectory => TempPathHelper.GetTempPath( "AssemblyLocator" );

        public virtual string SettingsDirectory => Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ), "Metalama" );
    }
}