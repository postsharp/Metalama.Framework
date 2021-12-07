// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Observers
{
    /// <summary>
    /// An interface that can be injected into the service provider to get callbacks from the <see cref="TemplateCompiler"/>
    /// class. For testing only.
    /// </summary>
    public interface ITemplateCompilerObserver : IService
    {
        /// <summary>
        /// Method invoked by the <see cref="TemplateCompiler.TryAnnotate"/> method.
        /// </summary>
        void OnAnnotatedSyntaxNode( SyntaxNode sourceSyntaxRoot, SyntaxNode annotatedSyntaxRoot );
    }
}