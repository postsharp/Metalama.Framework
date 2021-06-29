// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Custom attribute 
    /// </summary>
    [AttributeUsage( AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Event )]
    public class IntroduceAttribute : AdviceAttribute
    {
        /// <summary>
        /// Gets or sets the name of the aspect layer into which the member will be introduced. The layer must have been defined in the implementation of the
        /// <see cref="IAspect.BuildAspectClass"/> method.
        /// </summary>
        [Obsolete( "Not implemented." )]
        public string? Layer { get; set; }
    }
}