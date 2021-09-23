// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Utilities;
using System;
using System.IO;

namespace Caravela.Framework.Impl.Options
{
    /// <summary>
    /// A default implementation of <see cref="IPathOptions"/>.
    /// </summary>
    public class DefaultPathOptions : IPathOptions
    {
        protected DefaultPathOptions() { }

        public static readonly DefaultPathOptions Instance = new();

        public virtual string CompileTimeProjectCacheDirectory => TempPathHelper.GetTempPath( "CompileTime" );

        public virtual string AssemblyLocatorCacheDirectory => TempPathHelper.GetTempPath( "AssemblyLocator" );

        public virtual string SettingsDirectory => Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ), "Caravela" );

        public virtual string CrashReportDirectory => TempPathHelper.GetTempPath( "CrashReports" );
    }
}