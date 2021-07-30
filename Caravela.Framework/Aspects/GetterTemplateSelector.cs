// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Aspects
{
    public readonly struct GetterTemplateSelector
    {
        public string? DefaultTemplate { get; }

        public string? EnumerableTemplate { get; }

        public string? EnumeratorTemplate { get; }

        public bool HasOnlyDefaultTemplate => this.EnumeratorTemplate == null && this.EnumeratorTemplate == null;

        public bool IsNull => this.DefaultTemplate == null && this.EnumeratorTemplate == null && this.EnumerableTemplate == null;

        public GetterTemplateSelector( string? defaultTemplate, string? enumerableTemplate = null, string? enumeratorTemplate = null )
        {
            this.DefaultTemplate = defaultTemplate;
            this.EnumerableTemplate = enumerableTemplate;
            this.EnumeratorTemplate = enumeratorTemplate;
        }

        public static implicit operator GetterTemplateSelector( string methodName ) => new( methodName );
    }
}