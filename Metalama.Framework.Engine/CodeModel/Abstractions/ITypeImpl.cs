﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel.Visitors;

namespace Metalama.Framework.Engine.CodeModel.Abstractions
{
    internal interface ITypeImpl : ISdkType, ICompilationElementImpl
    {
        IType Accept( TypeRewriter visitor );
    }
}