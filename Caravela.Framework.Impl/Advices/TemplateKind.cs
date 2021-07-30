// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

// ReSharper disable InconsistentNaming

namespace Caravela.Framework.Impl.Advices
{
    internal enum TemplateKind
    {
        /// <summary>
        /// Not a template.
        /// </summary>
        None,

        /// <summary>
        /// Default template.
        /// </summary>
        Default,
        Async,
        IEnumerable,
        IEnumerator,
        IAsyncEnumerable,
        IAsyncEnumerator,

        /// <summary>
        /// Templated used for an introduction. Can be any type, must be detected from the signature and implementation.
        /// </summary>
        Introduction
    }
}