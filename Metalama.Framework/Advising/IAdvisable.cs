﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Advising;

[InternalImplement]
[CompileTime]
[PublicAPI]
public interface IAdvisable<out T>
{
    T Target { get; }

    IAdvisable<TNewDeclaration> WithTarget<TNewDeclaration>( TNewDeclaration target )
        where TNewDeclaration : IDeclaration;
}