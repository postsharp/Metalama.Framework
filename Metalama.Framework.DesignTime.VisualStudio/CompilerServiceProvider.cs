// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.DesignTime.VisualStudio.Preview;
using Metalama.Framework.Engine.Formatting;
using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.VisualStudio
{
    /// <summary>
    /// The implementation of <see cref="ICompilerServiceProvider"/>.
    /// </summary>
    internal class CompilerServiceProvider : ICompilerServiceProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public CompilerServiceProvider( IServiceProvider serviceProvider )
        {
            this._serviceProvider = serviceProvider;
            this.Version = this.GetType().Assembly.GetName().Version;
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
                return new UserProcessTransformationPreviewService( VisualStudioServiceProviderFactory.GetServiceProvider() );
            }
            else if ( type.IsEquivalentTo( typeof(ICompileTimeEditingStatusService) ) )
            {
                return new CompileTimeEditingStatusService( this._serviceProvider );
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