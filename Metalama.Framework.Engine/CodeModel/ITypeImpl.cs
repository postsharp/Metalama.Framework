﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

namespace Metalama.Framework.Engine.CodeModel
{
    internal interface ITypeImpl : ISdkType, ICompilationElementImpl
    {
        ITypeImpl Accept( TypeRewriter visitor );
    }
}