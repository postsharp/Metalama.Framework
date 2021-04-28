// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Sdk;
using System.Reflection;

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
        public ICodeElement CodeElement { get; }

        public AspectClassMetadata AspectClass { get; }

        IAspectClassMetadata IAspectInstance.AspectClass => this.AspectClass;

        internal AspectInstance( IAspect aspect, ICodeElement codeElement, AspectClassMetadata aspectClassMetadata )
        {
            this.Aspect = aspect;
            this.CodeElement = codeElement;
            this.AspectClass = aspectClassMetadata;
        }

        public MethodInfo? GetTemplateMethod( string methodName )
        {
            var aspectType = this.Aspect.GetType();

            return aspectType.GetMethod( methodName );
        }
    }
}