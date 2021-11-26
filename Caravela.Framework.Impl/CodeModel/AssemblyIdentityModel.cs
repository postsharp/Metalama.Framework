// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.Utilities.Dump;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class AssemblyIdentityModel : IAssemblyIdentity, IDumpable
    {
        private readonly AssemblyIdentity _assemblyIdentity;

        public AssemblyIdentityModel( AssemblyIdentity assemblyIdentity )
        {
            this._assemblyIdentity = assemblyIdentity;
        }

        public string Name => this._assemblyIdentity.Name;

        public Version Version => this._assemblyIdentity.Version;

        public string CultureName => this._assemblyIdentity.CultureName;

        public ImmutableArray<byte> PublicKey => this._assemblyIdentity.PublicKey;

        public ImmutableArray<byte> PublicKeyToken => this._assemblyIdentity.PublicKeyToken;

        public bool IsStrongNamed => this._assemblyIdentity.IsStrongName;

        public bool HasPublicKey => this._assemblyIdentity.HasPublicKey;

        public object ToDump() => this.ToDumpImpl();
    }
}