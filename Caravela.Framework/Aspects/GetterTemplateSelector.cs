// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections;
using System.Collections.Generic;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Specifies the templates that must be used for the <c>get</c> accessor by the <see cref="IAdviceFactory.OverrideFieldOrPropertyAccessors"/> advice.
    /// </summary>
    public readonly struct GetterTemplateSelector
    {
        /// <summary>
        /// Gets the name of the template that must be applied if no other template is applicable. This property is required if you want to
        /// override the getter.
        /// </summary>
        public string? DefaultTemplate { get; }

        /// <summary>
        /// Gets the name of the template that must be applied to iterator getters, including getters returning an enumerator.
        /// </summary>
        public string? IteratorTemplate { get; }

        /// <summary>
        /// Gets the name of the template that must be applied to an iterator method returning an <see cref="IEnumerator{T}"/> or <see cref="IEnumerator"/> (but not an async iterator).
        /// </summary>
        public string? IteratorEnumeratorTemplate { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IteratorTemplate"/> or <see cref="IteratorEnumeratorTemplate"/>
        /// must be applied to all methods returning compatible return type (if set to <c>true</c>), instead of only to methods using the <c>yield</c> statement.
        /// </summary>
        public bool UseIteratorTemplateForAnyEnumerable { get; }

        internal bool HasOnlyDefaultTemplate => this.IteratorEnumeratorTemplate == null && this.IteratorEnumeratorTemplate == null;

        internal bool IsNull => this.DefaultTemplate == null;

        /// <summary>
        /// Initializes a new instance of the <see cref="GetterTemplateSelector"/> struct by specifying the name of the template methods to be applied. The named passed to this
        /// constructor must be the name of methods of the current aspect class, and these methods must be annotated with the <see cref="TemplateAttribute"/> custom attribute.
        /// You can define several templates by passing a value to optional parameters. The appropriate template will be automatically selected according to
        /// the method to which the advice is applied. If several templates are eligible for a method, the template that is the last in the list of parameters is selected.
        /// </summary>
        /// <param name="defaultTemplate">Name of the template that must be applied if no other template is applicable. This parameter is required.</param>
        /// <param name="iteratorTemplate">Name of the template that must be applied to iterator methods, including methods returning an enumerator, and
        /// including async iterators.</param>
        /// <param name="iteratorEnumeratorTemplate">Name of the template that must be applied to an iterator method returning an an <see cref="IEnumerator{T}"/> or <see cref="IEnumerator"/> (but not an async iterator).</param>
        /// <param name="useIteratorTemplateForAnyEnumerable">Indicates whether the <see cref="IteratorTemplate"/> or <see cref="IteratorEnumeratorTemplate"/>
        /// must be applied to all methods returning compatible return type (if set to <c>true</c>), instead of only to methods using the <c>yield</c> statement..</param>
        /// <remarks>
        /// Note that this type has also an implicit conversion from <see cref="string"/>.
        /// If you only want to specify a default template, you can pass a string, without calling the constructor.
        /// </remarks>
        public GetterTemplateSelector(
            string defaultTemplate,
            string? iteratorTemplate = null,
            string? iteratorEnumeratorTemplate = null,
            bool useIteratorTemplateForAnyEnumerable = false )
        {
            this.DefaultTemplate = defaultTemplate;
            this.UseIteratorTemplateForAnyEnumerable = useIteratorTemplateForAnyEnumerable;
            this.IteratorTemplate = iteratorTemplate;
            this.IteratorEnumeratorTemplate = iteratorEnumeratorTemplate;
        }

        private GetterTemplateSelector( string? defaultTemplate )
        {
            this.DefaultTemplate = defaultTemplate;
            this.UseIteratorTemplateForAnyEnumerable = false;
            this.IteratorTemplate = null;
            this.IteratorEnumeratorTemplate = null;
        }

        /// <summary>
        /// Converts a <see cref="string"/> to a new instance of the <see cref="GetterTemplateSelector"/> where the <see cref="DefaultTemplate"/> property is
        /// set to this string.
        /// </summary>
        /// <param name="defaultTemplate">Name of the default template.</param>
        /// <returns></returns>
        public static implicit operator GetterTemplateSelector( string? defaultTemplate ) => new( defaultTemplate );
    }
}