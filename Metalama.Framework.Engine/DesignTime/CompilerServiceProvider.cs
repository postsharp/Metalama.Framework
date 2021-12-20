// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.Engine.DesignTime.Preview;
using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.DesignTime
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