// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// An interface that can be injected into the service provider to get callbacks from the <see cref="CompileTimeCompilationBuilder"/>
    /// class. For testing only.
    /// </summary>
    public interface ICompileTimeCompilationBuilderSpy
    {
        /// <summary>
        /// Method called by <see cref="CompileTimeCompilationBuilder.TryCreateCompileTimeCompilation"/>.
        /// </summary>
        void ReportCompileTimeCompilation( Compilation compilation );
    }
}