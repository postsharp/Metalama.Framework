// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Comparers;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.CodeModel;

/// <summary>
/// Contains the subset of <see cref="CSharpParseOptions"/> that must be identical for all syntax trees in the compilation.
/// </summary>
internal class LanguageOptions : IEquatable<LanguageOptions>
{
    private static readonly StructuralDictionaryComparer<string, string> _featureComparer = new( EqualityComparer<string>.Default );

    public LanguageVersion Version { get; }

    public ImmutableDictionary<string, string> Features { get; }

    private LanguageOptions( LanguageVersion version, ImmutableDictionary<string, string> features )
    {
        this.Version = version;
        this.Features = features;
    }

    public static LanguageOptions Default { get; } = new( LanguageVersion.Default, ImmutableDictionary<string, string>.Empty );

    internal LanguageOptions( CSharpParseOptions options ) : this( options.LanguageVersion, options.Features.ToImmutableDictionary() ) { }

    internal CSharpParseOptions ToParseOptions() => CSharpParseOptions.Default.WithLanguageVersion( this.Version ).WithFeatures( this.Features );

    public bool Equals( LanguageOptions? other )
    {
        if ( ReferenceEquals( null, other ) )
        {
            return false;
        }

        if ( ReferenceEquals( this, other ) )
        {
            return true;
        }

        return this.Version == other.Version && _featureComparer.Equals( this.Features, other.Features );
    }

    public override bool Equals( object? obj )
    {
        if ( ReferenceEquals( null, obj ) )
        {
            return false;
        }

        if ( ReferenceEquals( this, obj ) )
        {
            return true;
        }

        if ( obj.GetType() != this.GetType() )
        {
            return false;
        }

        return this.Equals( (LanguageOptions) obj );
    }

    public override int GetHashCode() => HashCode.Combine( (int) this.Version, _featureComparer.GetHashCode( this.Features ) );

    public static bool operator ==( LanguageOptions? left, LanguageOptions? right ) => Equals( left, right );

    public static bool operator !=( LanguageOptions? left, LanguageOptions? right ) => !Equals( left, right );
}