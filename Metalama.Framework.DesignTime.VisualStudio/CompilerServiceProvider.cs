// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.DesignTime.VisualStudio.Classification;
using Metalama.Framework.DesignTime.VisualStudio.Preview;

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

        public T? GetService<T>()
            where T : class, ICompilerService
        {
            var type = typeof(T);

            object? service;

            if ( type.IsEquivalentTo( typeof(IClassificationService) ) )
            {
                service = new DesignTimeClassificationService();
            }
            else if ( type.IsEquivalentTo( typeof(ITransformationPreviewService) ) )
            {
                service = new UserProcessTransformationPreviewService( VsServiceProviderFactory.GetServiceProvider() );
            }
            else if ( type.IsEquivalentTo( typeof(ICompileTimeEditingStatusService) ) )
            {
                service = new CompileTimeEditingStatusService( this._serviceProvider );
            }
            else
            {
                service = null;
            }

            return (T?) service;
        }

        event Action? ICompilerServiceProvider.Unloaded
        {
            add { }
            remove { }
        }
    }
}