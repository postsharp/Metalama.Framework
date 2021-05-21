// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using Caravela.Framework.Impl.Pipeline;
using System;

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

        public T? GetCompilerService<T>()
            where T : class, ICompilerService
            => typeof(T) == typeof(IClassificationService) ? (T) (object) new ClassificationService( ServiceProviderFactory.Shared ) : null;

        event Action<ICompilerServiceProvider>? ICompilerServiceProvider.Unloaded
        {
            add { }
            remove { }
        }
    }
}