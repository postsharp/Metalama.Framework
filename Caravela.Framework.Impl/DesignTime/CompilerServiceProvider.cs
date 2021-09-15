// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.ServiceProvider;
using Caravela.Framework.Impl.Utilities;
using System;
using System.Diagnostics;
using System.IO;

namespace Caravela.Framework.Impl.DesignTime
{
    /// <summary>
    /// The implementation of <see cref="ICompilerServiceProvider"/>.
    /// </summary>
    internal class CompilerServiceProvider : ICompilerServiceProvider
    {
        private static readonly CompilerServiceProvider _instance = new();

        static CompilerServiceProvider()
        {
            DesignTimeEntryPointManager.Instance.RegisterServiceProvider( _instance );

            // Also configure logging.
            // TODO: Move to Microsoft.Extensions.Logging.

            InitializeLogging();
        }

        private CompilerServiceProvider()
        {
            this.Version = this.GetType().Assembly.GetName().Version;
        }

        public static void Initialize()
        {
            // Make sure the type is initialized.
            _ = _instance.GetType();
        }

        private static void InitializeLogging()
        {
            var pid = Process.GetCurrentProcess().Id;

            var directory = Path.Combine( Path.GetTempPath(), "Caravela", "Logs" );

            try
            {
                RetryHelper.Retry(
                    () =>
                    {
                        if ( !Directory.Exists( directory ) )
                        {
                            Directory.CreateDirectory( directory );
                        }
                    } );

                var textWriter = File.CreateText( Path.Combine( directory, $"Caravela.{Process.GetCurrentProcess().ProcessName}.{pid}.log" ) );

                Logger.Initialize( textWriter );
            }
            catch
            {
                // Don't fail if we cannot initialize the log.
            }
        }

        public Version Version { get; }

        public T? GetCompilerService<T>()
            where T : class, ICompilerService
            => typeof(T) == typeof(IClassificationService) ? (T) (object) new ClassificationService( ServiceProviderFactory.GlobalProvider ) : null;

        event Action? ICompilerServiceProvider.Unloaded
        {
            add { }
            remove { }
        }
    }
}