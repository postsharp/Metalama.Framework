// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using System;

namespace Metalama.Framework.Engine.AspectWeavers
{
    /// <summary>
    /// Custom attribute that, when applied to a type implementing <see cref="IAspectWeaver"/>, exports this type
    /// as an aspect weaver. Additionally, aspect weavers must have the <see cref="MetalamaPlugInAttribute"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public sealed class AspectWeaverAttribute : Attribute
    {
        /// <summary>
        /// Gets the type of aspects that this weaver handles.
        /// </summary>
        public Type AspectType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AspectWeaverAttribute"/> class and specifies the type
        /// of aspects handled by this weaver.
        /// </summary>
        /// <param name="aspectType">The type of aspects handled by this weaver.</param>
        public AspectWeaverAttribute( Type aspectType )
        {
            this.AspectType = aspectType;
        }
    }
}