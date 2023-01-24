// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel;
using System.Linq;

namespace Metalama.Framework.Engine.Advising
{
    internal sealed class TemplateTypeRewriter : TypeRewriter
    {
        private readonly BoundTemplateMethod _template;

        private TemplateTypeRewriter( BoundTemplateMethod template )
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