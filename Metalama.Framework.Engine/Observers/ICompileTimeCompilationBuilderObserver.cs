// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Observers
{
    /// <summary>
    /// An interface that can be injected into the service provider to get callbacks from the <see cref="CompileTimeCompilationBuilder"/>
    /// class. For testing only.
    /// </summary>
    public interface ICompileTimeCompilationBuilderObserver : IService
    {
        /// <summary>
        /// Method called by <see cref="CompileTimeCompilationBuilder.TryCreateCompileTimeCompilation"/>.
        /// </summary>
        void OnCompileTimeCompilation( Compilation compilation );

        void OnCompileTimeCompilationEmit( Compilation compilation, ImmutableArray<Diagnostic> diagnostics );
    }
}