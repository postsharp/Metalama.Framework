// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Custom attribute that can be applied to any member of an aspect class and that means that this member must be introduced to
    /// the target class of the aspect. 
    /// </summary>
    /// <seealso href="@introducing-members"/>
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Event )]
    public class IntroduceAttribute : TemplateAttribute
    {
        public IntroduceAttribute() : base( TemplateKind.Introduction ) { }

        /// <summary>
        /// Gets or sets the name of the aspect layer into which the member will be introduced. The layer must have been defined in the implementation of the
        /// <see cref="IAspect.BuildAspectClass"/> method.
        /// </summary>
        [Obsolete( "Not implemented." )]
        public string? Layer { get; set; }
    }
}