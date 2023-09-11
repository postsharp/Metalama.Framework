﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Utilities.Roslyn;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel;

internal sealed class ResolvingCompileTimeTypeFactory : CompileTimeTypeFactory
{
    private readonly SerializableTypeIdResolver _serializableTypeIdResolver;

    public ResolvingCompileTimeTypeFactory( SerializableTypeIdResolver serializableTypeIdResolver )
    {
        this._serializableTypeIdResolver = serializableTypeIdResolver;
    }

    public Type Get( SerializableTypeId typeId, IReadOnlyDictionary<string, IType>? substitutions )
        => this.Get( this._serializableTypeIdResolver.ResolveId( typeId, substitutions ) );
}