// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

// ReSharper disable InconsistentNaming

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Enumeration of the kind of templates that were specified by the user using <see cref="GetterTemplateSelector"/> or
    /// <see cref="TemplateKind"/>. A <see cref="MethodTemplateSelector"/> represents the intention of the user, not
    /// a characteristic of the declaration used as a template.
    /// </summary>
    public enum TemplateKind
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
        /// <see cref="MethodTemplateSelector.EnumerableTemplate"/> or <see cref="GetterTemplateSelector.EnumerableTemplate"/>.
        /// </summary>
        IEnumerable,

        /// <summary>
        /// <see cref="MethodTemplateSelector.EnumeratorTemplate"/> or <see cref="GetterTemplateSelector.EnumeratorTemplate"/>.
        /// </summary>
        IEnumerator,

        /// <summary>
        /// <see cref="MethodTemplateSelector.AsyncEnumerableTemplate"/>.
        /// </summary>
        IAsyncEnumerable,

        /// <summary>
        /// <see cref="MethodTemplateSelector.AsyncEnumeratorTemplate"/>.
        /// </summary>
        IAsyncEnumerator,

        /// <summary>
        /// Template used for an introduction. Can be any type, must be detected from the signature and implementation.
        /// </summary>
        Introduction,

        /// <summary>
        /// Template used for initializer of introduced field, property of event field.
        /// </summary>
        Initializer,
    }
}