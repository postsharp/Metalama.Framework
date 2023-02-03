// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class AssemblyIdentityModel : IAssemblyIdentity
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

        public override string ToString() => this._assemblyIdentity.ToString();

        public bool Equals( IAssemblyIdentity? other )
        {
            if ( ReferenceEquals( null, other ) )
            {
                return false;
            }

            if ( ReferenceEquals( this, other ) )
            {
                return true;
            }

            return this._assemblyIdentity.Equals( ((AssemblyIdentityModel) other)._assemblyIdentity );
        }

        public override bool Equals( object? obj ) => ReferenceEquals( this, obj ) || ( obj is IAssemblyIdentity other && this.Equals( other ) );

        public override int GetHashCode() => this._assemblyIdentity.GetHashCode();
    }
}