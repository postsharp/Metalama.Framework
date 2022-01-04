// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Metalama.Framework.Aspects
{
    /// <summary>
    /// Custom attribute that can be applied to any member of an aspect class and that means that this member must be introduced to
    /// the target class of the aspect. 
    /// </summary>
    /// <seealso href="@introducing-members"/>
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Event )]
    public class IntroduceAttribute : TemplateAttribute
    {
        /// <summary>
        /// Gets or sets the name of the aspect layer into which the member will be introduced. The layer must have been defined
        /// using the <see cref="LayersAttribute"/> custom attribute.
        /// </summary>
        [Obsolete( "Not implemented." )]
        public string? Layer { get; set; }
    }
}