// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Validation;
using System;
using System.Collections.Immutable;

namespace Caravela.Framework.Code
{
    [InternalImplement]
    [CompileTimeOnly]
    public interface IAssemblyIdentity
    {
        string Name { get; }

        Version Version { get; }

        string CultureName { get; }

        ImmutableArray<byte> PublicKey { get; }

        ImmutableArray<byte> PublicKeyToken { get; }

        bool IsStrongName { get; }

        bool HasPublicKey { get; }
    }
}