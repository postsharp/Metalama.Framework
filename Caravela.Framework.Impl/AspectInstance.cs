// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Templating;
using Caravela.Framework.Sdk;
using System;
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

        public TemplateDriver GetTemplateDriver( ICodeElement sourceTemplate )
        {
            var aspectType = this.Aspect.GetType();
            MethodInfo? compiledTemplateMethodInfo;

            switch ( sourceTemplate )
            {
                case IMethod method:
                    var methodName = method.Name + TemplateCompiler.TemplateMethodSuffix;
                    compiledTemplateMethodInfo = aspectType.GetMethod( methodName );

                    break;

                default:
                    throw new NotImplementedException();
            }

            if ( compiledTemplateMethodInfo == null )
            {
                throw new AssertionFailedException( $"Could not find the compile template for {sourceTemplate}." );
            }

            return new TemplateDriver( sourceTemplate.GetSymbol().AssertNotNull(), compiledTemplateMethodInfo );
        }
    }
}