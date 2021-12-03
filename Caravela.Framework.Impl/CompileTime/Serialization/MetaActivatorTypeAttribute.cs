// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Impl.CompileTime.Serialization
{
    /// <summary>
    /// Custom attribute that, when applied to an assembly, points to a type in the assembly implementing <see cref="IMetaActivator"/>.
    /// </summary>
    [AttributeUsage( AttributeTargets.Assembly )]
    public sealed class MetaActivatorTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetaActivatorTypeAttribute"/> class.
        /// </summary>
        /// <param name="activatorType">A type derived from <see cref="IMetaActivator"/> in the current assembly. This type must be public and have
        /// a default constructor.</param>
        public MetaActivatorTypeAttribute( Type activatorType )
        {
            this.ActivatorType = activatorType;
        }

        /// <summary>
        /// Gets the activator type.
        /// </summary>
        public Type ActivatorType { get; private set; }
    }
}