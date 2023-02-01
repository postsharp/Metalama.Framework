// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities.Roslyn;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel;

internal partial class ResolvingCompileTimeTypeFactory : CompileTimeTypeFactory
{
    private readonly SerializableTypeIdResolver _serializableTypeIdResolver;

    public ResolvingCompileTimeTypeFactory( SerializableTypeIdResolver serializableTypeIdResolver )
    {
        this._serializableTypeIdResolver = serializableTypeIdResolver;
    }

    public Type Get( SerializableTypeId typeId, IReadOnlyDictionary<string, IType>? substitutions )
    {
        var originalSymbol = this._serializableTypeIdResolver.ResolveId( typeId );

        if ( originalSymbol == null )
        {
            throw new ArgumentOutOfRangeException( nameof(typeId), $"Cannot resolve the type '{typeId}'" );
        }

        if ( substitutions is { Count: > 0 } )
        {
            var compilation = substitutions.First().Value.GetCompilationModel();
            var originalType = compilation.Factory.GetIType( originalSymbol );
            var rewriter = new TypeParameterRewriter( substitutions );
            var rewrittenTypeSymbol = rewriter.Visit( originalType ).GetSymbol();

            return this.Get( rewrittenTypeSymbol );
        }
        else
        {
            return this.Get( originalSymbol );
        }
    }
}