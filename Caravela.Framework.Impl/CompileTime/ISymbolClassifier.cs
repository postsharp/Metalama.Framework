// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// Determines the kind of symbol: template, <see cref="TemplatingScope.CompileTimeOnly"/>,
    /// <see cref="TemplatingScope.RunTimeOnly"/>.
    /// </summary>
    internal interface ISymbolClassifier
    {
        TemplateInfo GetTemplateInfo( ISymbol symbol );

        /// <summary>
        /// Gets the scope of a symbol in the context of a template.
        /// </summary>
        TemplatingScope GetTemplatingScope( ISymbol symbol );
    }

    internal readonly struct TemplateInfo
    {
        public bool IsNone => this.AttributeType == TemplateAttributeType.None;

        public bool IsAbstract { get; }

        public TemplateAttributeType AttributeType { get; }

        public TemplateKind Kind { get; }

        public TemplateInfo( TemplateAttributeType attributeType, TemplateKind kind ) : this( attributeType, kind, false ) { }

        private TemplateInfo( TemplateAttributeType attributeType, TemplateKind kind, bool isAbstract )
        {
            this.AttributeType = attributeType;
            this.Kind = kind;
            this.IsAbstract = isAbstract;
        }

        public TemplateInfo AsAbstract() => new( this.AttributeType, this.Kind, true );
    }
}