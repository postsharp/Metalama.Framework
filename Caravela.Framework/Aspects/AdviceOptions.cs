// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;
using System.ComponentModel;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Options passed to methods of the <see cref="IAdviceFactory"/> interface. Instances of this class are immutable.
    /// </summary>
    public sealed class AdviceOptions
    {
        /// <summary>
        /// Gets the default options.
        /// </summary>
        public static AdviceOptions Default { get; } = new( null, ImmutableDictionary<string, object?>.Empty );

        internal AspectLinkerOptions? LinkerOptions { get; }

        /// <summary>
        /// Gets the dictionary of tags.
        /// </summary>
        public ImmutableDictionary<string, object?> Tags { get; }

        internal AdviceOptions( AspectLinkerOptions? linkerOptions, ImmutableDictionary<string, object?> tags )
        {
            this.LinkerOptions = linkerOptions;
            this.Tags = tags;
        }

        /// <exclude/>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public AdviceOptions WithLinkerOptions( bool forceNotInlineable ) => new( AspectLinkerOptions.Create( forceNotInlineable ), this.Tags );

        /// <summary>
        /// Returns an <see cref="AdviceOptions"/> object with an additional tag value. This tag value can be read
        /// by the advice using the <see cref="meta.Tags"/> property of the <see cref="meta"/> API.
        /// </summary>
        /// <param name="name">Tag name.</param>
        /// <param name="value">Tag value.</param>
        /// <returns>A new instance of <see cref="AdviceOptions"/>.</returns>
        public AdviceOptions AddTag( string name, object? value ) => new( this.LinkerOptions, this.Tags.Add( name, value ) );

        /// <summary>
        /// Returns an <see cref="AdviceOptions"/> with a given set of tags. These tags  can be read
        /// by the advice using the <see cref="meta.Tags"/> property of the <see cref="meta"/> API.
        /// </summary>
        /// <param name="tags">A dictionary of tags.</param>
        /// <returns>A new instance of <see cref="AdviceOptions"/>.</returns>
        public AdviceOptions WithTags( ImmutableDictionary<string, object?> tags ) => new( this.LinkerOptions, tags );
    }
}