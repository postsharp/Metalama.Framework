// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Project;
using System;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// Represents the implementation of an <see cref="IService"/> that is lazily created and, therefore, can have circular dependencies
    /// with other late-bound services. Late-bound services can be registered using <see cref="ServiceProvider.WithLateBoundServices"/>.
    /// </summary>
    internal sealed class LateBoundService
    {
        public Type Type { get; }

        public Func<IServiceProvider, IService> CreateFunc { get; }

        private LateBoundService( Type type, Func<IServiceProvider, IService> createFunc )
        {
            this.Type = type;
            this.CreateFunc = createFunc;
        }

        public static LateBoundService Create<T>( Func<IServiceProvider, T> func )
            where T : IService
            => new( typeof(T), x => func( x ) );
    }
}