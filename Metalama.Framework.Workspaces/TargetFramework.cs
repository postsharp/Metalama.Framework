// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Workspaces
{
    /// <summary>
    /// Represents a target framework such as <c>net6.0</c> or <c>netstandard2.0</c>.
    /// </summary>
    public readonly struct TargetFramework : IEquatable<TargetFramework>
    {
        public string? Id { get; }

        public bool Equals( TargetFramework other ) => this.Id == other.Id;

        public override bool Equals( object? obj ) => obj is TargetFramework other && this.Equals( other );

        public override int GetHashCode() => this.Id?.GetHashCode( StringComparison.Ordinal ) ?? 0;

        public TargetFramework( string? id )
        {
            this.Id = id;
        }

        public static implicit operator TargetFramework( string? framework ) => new( framework );

        public static bool operator ==( TargetFramework a, TargetFramework b ) => a.Equals( b );

        public static bool operator !=( TargetFramework a, TargetFramework b ) => !a.Equals( b );

        public override string ToString() => this.Id ?? "null";
    }
}