﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;

namespace Metalama.Framework.Engine.CompileTime;

internal interface ISystemTypeResolverFactory : IGlobalService
{
    SystemTypeResolver Create( ProjectServiceProvider serviceProvider, CompilationContext compilationContext );
}