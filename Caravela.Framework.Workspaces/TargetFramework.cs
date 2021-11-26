// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Workspaces
{
    public readonly struct TargetFramework : IEquatable<TargetFramework>
    {
        public bool Equals( TargetFramework other ) => this._value == other._value;

        public override bool Equals( object? obj ) => obj is TargetFramework other && this.Equals( other );

        public override int GetHashCode() => this._value?.GetHashCode( StringComparison.Ordinal ) ?? 0;

        private readonly string _value;

        public TargetFramework( string value )
        {
            this._value = value;
        }

        public static implicit operator TargetFramework( string framework ) => new( framework );

        public static bool operator ==( TargetFramework a, TargetFramework b ) => a.Equals( b );

        public static bool operator !=( TargetFramework a, TargetFramework b ) => !a.Equals( b );

        public override string ToString() => this._value;

        // ReSharper disable once UnusedMember.Local
        private object ToDump() => this._value;
    }
}