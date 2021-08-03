// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

// ReSharper disable InconsistentNaming

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Impl.Advices
{
    /// <summary>
    /// Enumeration of the kind of templates that were specified by the user using <see cref="MethodTemplateSelector"/> or
    /// <see cref="GetterTemplateSelector"/>. A <see cref="TemplateSelectionKind"/> represents the intention of the user, not
    /// a characteristic of the declaration used as a template.
    /// </summary>
    internal enum TemplateSelectionKind
    {
        /// <summary>
        /// Not a template.
        /// </summary>
        None,

        /// <summary>
        /// Default template.
        /// </summary>
        Default,

        /// <summary>
        /// <see cref="MethodTemplateSelector.AsyncTemplate"/>.
        /// </summary>
        Async,

        /// <summary>
        /// <see cref="MethodTemplateSelector.IteratorTemplate"/> or <see cref="GetterTemplateSelector.IteratorTemplate"/>.
        /// </summary>
        IEnumerable,

        /// <summary>
        /// <see cref="MethodTemplateSelector.IteratorEnumeratorTemplate"/> or <see cref="GetterTemplateSelector.IteratorEnumeratorTemplate"/>.
        /// </summary>
        IEnumerator,

        /// <summary>
        /// <see cref="MethodTemplateSelector.AsyncIteratorTemplate"/>.
        /// </summary>
        IAsyncEnumerable,

        /// <summary>
        /// <see cref="MethodTemplateSelector.AsyncIteratorEnumeratorTemplate"/>.
        /// </summary>
        IAsyncEnumerator,

        /// <summary>
        /// Templated used for an introduction. Can be any type, must be detected from the signature and implementation.
        /// </summary>
        Introduction
    }
}