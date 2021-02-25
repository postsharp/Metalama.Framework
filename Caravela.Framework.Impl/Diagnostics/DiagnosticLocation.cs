using System;
using Caravela.Framework.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Diagnostics
{
    /// <summary>
    /// Wraps a Roslyn <see cref="Location"/> into a <see cref="DiagnosticLocation"/>.
    /// </summary>
    internal class DiagnosticLocation : IDiagnosticLocation, IEquatable<DiagnosticLocation>
    {
        public DiagnosticLocation( Location? location )
        {
            this.Location = location;
        }

        public Location? Location { get; }

        public bool Equals( DiagnosticLocation? other )
        {
            if ( other == null )
            {
                return false;
            }

            if ( ReferenceEquals( this, other ) || ReferenceEquals( this.Location, other.Location ) )
            {
                return true;
            }

            if ( this.Location == null )
            {
                return other.Location == null;
            }
            else if ( other.Location == null )
            {
                return false;
            }

            return this.Location.SourceTree == other.Location.SourceTree &&
                   this.Location.SourceSpan == other.Location.SourceSpan;
        }

        bool IEquatable<IDiagnosticLocation?>.Equals( IDiagnosticLocation? other )
        {
            return this.Equals( other );
        }

        public override bool Equals( object? obj )
        {
            return obj is DiagnosticLocation other && this.Equals( other );
        }

        public override int GetHashCode()
        {
            return this.Location != null ? this.Location.GetHashCode() : 0;
        }

        public static bool operator ==( DiagnosticLocation? left, DiagnosticLocation? right ) => Equals( left, right );

        public static bool operator !=( DiagnosticLocation? left, DiagnosticLocation? right ) => !Equals( left, right );
    }
}