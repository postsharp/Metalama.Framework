// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Sdk;

namespace Caravela.Framework.Impl
{
    /// <summary>
    /// Represents an instance of an aspect and its target code element.
    /// </summary>
    internal sealed class AspectInstance : IAspectInstance
    {
        /// <summary>
        /// Gets the aspect instance.
        /// </summary>
        public IAspect Aspect { get; }

        /// <summary>
        /// Gets the element of code to which the aspect is applied.
        /// </summary>
        public IDeclaration Declaration { get; }

        public AspectClassMetadata AspectClass { get; }

        IAspectClassMetadata IAspectInstance.AspectClass => this.AspectClass;

        internal AspectInstance( IAspect aspect, IDeclaration declaration, AspectClassMetadata aspectClassMetadata )
        {
            this.Aspect = aspect;
            this.Declaration = declaration;
            this.AspectClass = aspectClassMetadata;
        }
    }
}