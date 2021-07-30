// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Aspects
{
    public readonly struct MethodTemplateSelector
    {
        public string DefaultTemplate { get; }

        public string? AsyncTemplate { get; }

        public string? EnumerableTemplate { get; }

        public string? EnumeratorTemplate { get; }

        public string? AsyncEnumerableTemplate { get; }

        public string? AsyncEnumeratorTemplate { get; }

        public bool UseAsyncTemplateForAnyAwaitable { get; }

        public bool UseAsyncTemplateForAnyEnumerable { get; }

        public bool HasOnlyDefaultTemplate
            => this.AsyncTemplate == null &&
               this.EnumerableTemplate == null &&
               this.EnumeratorTemplate == null &&
               this.AsyncEnumerableTemplate == null
               && this.AsyncEnumeratorTemplate == null;

        public MethodTemplateSelector(
            string defaultTemplate,
            string? asyncTemplate = null,
            string? enumerableTemplate = null,
            string? enumeratorTemplate = null,
            string? asyncEnumerableTemplate = null,
            string? asyncEnumeratorTemplate = null,
            bool useAsyncTemplateForAnyAwaitable = false,
            bool useAsyncTemplateForAnyEnumerable = false )
        {
            this.DefaultTemplate = defaultTemplate;
            this.UseAsyncTemplateForAnyAwaitable = useAsyncTemplateForAnyAwaitable;
            this.UseAsyncTemplateForAnyEnumerable = useAsyncTemplateForAnyEnumerable;
            this.AsyncTemplate = asyncTemplate;
            this.EnumerableTemplate = enumerableTemplate;
            this.EnumeratorTemplate = enumeratorTemplate;
            this.AsyncEnumerableTemplate = asyncEnumerableTemplate;
            this.AsyncEnumeratorTemplate = asyncEnumeratorTemplate;
        }

        public static implicit operator MethodTemplateSelector( string methodName ) => new( methodName );
    }
}