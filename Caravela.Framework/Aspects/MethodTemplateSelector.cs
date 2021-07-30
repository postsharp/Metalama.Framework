// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections;
using System.Collections.Generic;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Specifies the templates that must be used by the <see cref="IAdviceFactory.OverrideMethod"/> advice.
    /// </summary>
    public readonly struct MethodTemplateSelector
    {
        /// <summary>
        /// Gets the name of the template that must be applied if no other template is applicable. This property is required.
        /// </summary>
        public string DefaultTemplate { get; }

        /// <summary>
        /// Gets the name of the template that must be applied to async methods, including async iterators.
        /// </summary>
        public string? AsyncTemplate { get; }

        /// <summary>
        /// Gets the name of the template that must be applied to iterator methods, including methods returning an enumerator, and including async iterators.
        /// </summary>
        public string? IteratorTemplate { get; }

        /// <summary>
        /// Gets the name of the template that must be applied to an iterator method returning an <see cref="IEnumerator{T}"/> or <see cref="IEnumerator"/> (but not an async iterator).
        /// </summary>
        public string? IteratorEnumeratorTemplate { get; }

        /// <summary>
        /// Gets the name of the template that must be applied to an async iterator method (<c>IAsyncEnumerable</c>), including an async enumerator (<c>IAsyncEnumerator</c>).
        /// </summary>
        public string? AsyncIteratorTemplate { get; }

        /// <summary>
        /// Gets the name of the template that must be applied to an async iterator method returning an <c>IAsyncEnumerator</c>.
        /// </summary>
        public string? AsyncIteratorEnumeratorTemplate { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="AsyncTemplate"/> must be applied to all methods returning an awaitable type (if set to <c>true</c>),
        /// instead of only to methods that have the <c>async</c> modifier.
        /// </summary>
        public bool UseAsyncTemplateForAnyAwaitable { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IteratorTemplate"/>, <see cref="IteratorEnumeratorTemplate"/>, <see cref="AsyncIteratorTemplate"/>,
        /// <see cref="AsyncIteratorEnumeratorTemplate"/> must be applied to all methods returning compatible return type (if set to <c>true</c>),
        /// instead of only to methods using the <c>yield</c> statement.
        /// </summary>
        public bool UseIteratorTemplateForAnyEnumerable { get; }

        internal bool HasOnlyDefaultTemplate
            => this.AsyncTemplate == null &&
               this.IteratorTemplate == null &&
               this.IteratorEnumeratorTemplate == null &&
               this.AsyncIteratorTemplate == null
               && this.AsyncIteratorEnumeratorTemplate == null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodTemplateSelector"/> struct by specifying the name of the template methods to be applied. The named passed to this
        /// constructor must be the name of methods of the current aspect class, and these methods must be annotated with the <see cref="TemplateAttribute"/> custom attribute.
        /// You can define several templates by passing a value to optional parameters. The appropriate template will be automatically selected according to
        /// the method to which the advice is applied. If several templates are eligible for a method, the template that is the last in the list of parameters is selected.
        /// </summary>
        /// <param name="defaultTemplate">Name of the template that must be applied if no other template is applicable. This parameter is required.</param>
        /// <param name="asyncTemplate">Name of the template that must be applied to async methods, including async iterators.</param>
        /// <param name="iteratorTemplate">Name of the template that must be applied to iterator methods, including methods returning an enumerator, and
        /// including async iterators.</param>
        /// <param name="iteratorEnumeratorTemplate">Name of the template that must be applied to an iterator method returning an an <see cref="IEnumerator{T}"/> or <see cref="IEnumerator"/> (but not an async iterator).</param>
        /// <param name="asyncIteratorTemplate">Name of the template that must be applied to an async iterator method,(<c>IAsyncEnumerable</c>), including an async enumerator (<c>IAsyncEnumerator</c>).</param>
        /// <param name="asyncIteratorEnumeratorTemplate">Name of the template that must be applied to an async iterator method returning <c>IAsyncEnumerator</c>.</param>
        /// <param name="useAsyncTemplateForAnyAwaitable">Indicates whether the <see cref="AsyncTemplate"/> must be applied to all methods returning an awaitable
        /// type (if set to <c>true</c>), instead of only to methods that have the <c>async</c> modifier.</param>
        /// <param name="useIteratorTemplateForAnyEnumerable">Indicates whether the <see cref="IteratorTemplate"/>, <see cref="IteratorEnumeratorTemplate"/>, <see cref="AsyncIteratorTemplate"/>,
        /// <see cref="AsyncIteratorEnumeratorTemplate"/> must be applied to all methods returning compatible return type (if set to <c>true</c>),
        /// instead of only to methods using the <c>yield</c> statement.</param>
        /// <remarks>
        /// Note that this type has also an implicit conversion from <see cref="string"/>.
        /// If you only want to specify a default template, you can pass a string, without calling the constructor.
        /// </remarks>
        public MethodTemplateSelector(
            string defaultTemplate,
            string? asyncTemplate = null,
            string? iteratorTemplate = null,
            string? iteratorEnumeratorTemplate = null,
            string? asyncIteratorTemplate = null,
            string? asyncIteratorEnumeratorTemplate = null,
            bool useAsyncTemplateForAnyAwaitable = false,
            bool useIteratorTemplateForAnyEnumerable = false )
        {
            this.DefaultTemplate = defaultTemplate;
            this.UseAsyncTemplateForAnyAwaitable = useAsyncTemplateForAnyAwaitable;
            this.UseIteratorTemplateForAnyEnumerable = useIteratorTemplateForAnyEnumerable;
            this.AsyncTemplate = asyncTemplate;
            this.IteratorTemplate = iteratorTemplate;
            this.IteratorEnumeratorTemplate = iteratorEnumeratorTemplate;
            this.AsyncIteratorTemplate = asyncIteratorTemplate;
            this.AsyncIteratorEnumeratorTemplate = asyncIteratorEnumeratorTemplate;
        }

        /// <summary>
        /// Converts a <see cref="string"/> to a new instance of the <see cref="MethodTemplateSelector"/> where the <see cref="DefaultTemplate"/> property is
        /// set to this string.
        /// </summary>
        /// <param name="defaultTemplate">Name of the default template.</param>
        /// <returns></returns>
        public static implicit operator MethodTemplateSelector( string defaultTemplate ) => new( defaultTemplate );
    }
}