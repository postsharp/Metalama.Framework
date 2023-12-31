﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel;

[PublicAPI]
public interface IReflectionMapper
{
    ITypeSymbol GetTypeSymbol( Type type );
}