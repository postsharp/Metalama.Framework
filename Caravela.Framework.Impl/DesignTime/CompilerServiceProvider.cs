// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.DesignTime.Contracts;
using System;

namespace Caravela.Framework.Impl.DesignTime
{
    internal class CompilerServiceProvider : ICompilerServiceProvider
    {
        public static readonly CompilerServiceProvider Instance = new();

        static CompilerServiceProvider()
        {
            DesignTimeEntryPointManager.Instance.RegisterServiceProvider( Instance );
        }

        public CompilerServiceProvider()
        {
            this.Version = this.GetType().Assembly.GetName().Version;
        }

        public static void Initialize()
        {
            // Make sure the type is initialized.
            _ = Instance.GetType();
        }

        public Version Version { get; }

        public T? GetCompilerService<T>()
            where T : class, ICompilerService
            => typeof(T) == typeof(IClassificationService) ? (T) (object) new ClassificationService() : null;

        event Action<ICompilerServiceProvider>? ICompilerServiceProvider.Unloaded
        {
            add { }
            remove { }
        }
    }
}