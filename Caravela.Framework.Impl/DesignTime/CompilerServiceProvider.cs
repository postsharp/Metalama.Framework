// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.DesignTime.Preview;
using Caravela.Framework.Impl.Pipeline;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

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

                // The filename must be unique because several instances of the current assembly (of different versions) may be loaded in the process.
                var textWriter = File.CreateText( Path.Combine( directory, $"Caravela.{Process.GetCurrentProcess().ProcessName}.{pid}.{Guid.NewGuid()}.log" ) );

                Logger.Initialize( textWriter );
            }
            catch
            {
                // Don't fail if we cannot initialize the log.
            }
        }

        public Version Version { get; }

        public ICompilerService? GetCompilerService( Type type )
        {
            if ( type.IsEquivalentTo( typeof(IClassificationService) ) )
            {
                return new ClassificationService( ServiceProviderFactory.GlobalProvider.WithProjectScopedServices( Enumerable.Empty<MetadataReference>() ) );
            }
            else if ( type.IsEquivalentTo( typeof(ITransformationPreviewService) ) )
            {
                return new TransformationPreviewService();
            }
            else
            {
                return null;
            }
        }

        event Action? ICompilerServiceProvider.Unloaded
        {
            add { }
            remove { }
        }
    }
}