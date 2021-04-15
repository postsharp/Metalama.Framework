// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Sdk
{
    /// <summary>
    /// Represents an instance of an aspect and its target code element.
    /// </summary>
    public sealed class AspectInstance
    {
        /// <summary>
        /// Gets the aspect instance.
        /// </summary>
        public IAspect Aspect { get; }

        /// <summary>
        /// Gets the element of code to which the aspect is applied.
        /// </summary>
        public ISdkCodeElement CodeElement { get; }

        internal INamedType AspectType { get; }

        internal AspectInstance( IAspect aspect, ISdkCodeElement codeElement, INamedType aspectType )
        {
            this.Aspect = aspect;
            this.CodeElement = codeElement;
            this.AspectType = aspectType;
        }
    }
}