// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel;

internal sealed partial class CompileTimeTypeFactory
{
    internal sealed class TypeParameterRewriter : TypeRewriter
    {
        private readonly IReadOnlyDictionary<string, IType> _substitutions;

        public TypeParameterRewriter( IReadOnlyDictionary<string, IType> substitutions )
        {
            this._substitutions = substitutions;
        }

        public static TypeRewriter Get( BoundTemplateMethod template )
        {
            return template.Template.TemplateClassMember.TypeParameters.All( x => !x.IsCompileTime )
                ? Null
                : new TemplateTypeRewriter( template );
        }

        internal override ITypeInternal Visit( TypeParameter typeParameter )
        {
            if ( this._substitutions.TryGetValue( typeParameter.Name, out var substitution ) )
            {
                return (ITypeInternal) substitution;
            }
            else
            {
                return typeParameter;
            }
        }
    }
}