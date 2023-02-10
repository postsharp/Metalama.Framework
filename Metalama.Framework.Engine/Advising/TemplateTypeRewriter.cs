﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.CodeModel;
using System.Linq;

namespace Metalama.Framework.Engine.Advising
{
    internal sealed class TemplateTypeRewriter : TypeRewriter
    {
        private readonly PartiallyBoundTemplateMethod _template;

        private TemplateTypeRewriter( PartiallyBoundTemplateMethod template )
        {
            this._template = template;
        }

        public static TypeRewriter Get( PartiallyBoundTemplateMethod template )
            => template.TemplateMember.TemplateClassMember.TypeParameters.All( x => !x.IsCompileTime )
                ? Null
                : new TemplateTypeRewriter( template );

        internal override ITypeInternal Visit( TypeParameter typeParameter )
        {
            if ( this._template.TemplateMember.TemplateClassMember.IndexedParameters.TryGetValue( typeParameter.Name, out var templateParameter )
                 && templateParameter.IsCompileTime )
            {
                var value = (TemplateTypeArgument) this._template.TypeArguments[templateParameter.TemplateIndex!.Value]!;

                return (ITypeInternal) value.Type;
            }
            else
            {
                return typeParameter;
            }
        }
    }
}