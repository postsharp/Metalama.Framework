// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Templating;
using System.Linq;

namespace Metalama.Framework.Engine.Advices
{
    internal class TemplateTypeRewriter : TypeRewriter
    {
        private readonly BoundTemplateMethod _template;

        public TemplateTypeRewriter( BoundTemplateMethod template )
        {
            this._template = template;
        }

        public static TypeRewriter Get( BoundTemplateMethod template )
            => template.Template.TemplateClassMember.TypeParameters.All( x => !x.IsCompileTime )
                ? Null
                : new TemplateTypeRewriter( template );

        internal override ITypeInternal Visit( TypeParameter typeParameter )
        {
            if ( this._template.Template.TemplateClassMember.IndexedParameters.TryGetValue( typeParameter.Name, out var templateParameter )
                 && templateParameter.IsCompileTime )
            {
                var value = (TemplateTypeArgument) this._template.TemplateArguments[templateParameter.TemplateIndex!.Value]!;

                return (ITypeInternal) value.Type;
            }
            else
            {
                return typeParameter;
            }
        }
    }
}