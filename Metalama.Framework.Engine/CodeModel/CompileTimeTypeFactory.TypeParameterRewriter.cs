﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;

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