// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.ServiceProvider;
using Caravela.Framework.Impl.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace Caravela.Framework.Impl.Aspects
{
    /// <summary>
    /// Represents an instance of an aspect and its target declaration.
    /// </summary>
    internal sealed class AspectInstance : IAspectInstance
    {
        /// <summary>
        /// Gets the aspect instance.
        /// </summary>
        public IAspect Aspect { get; }

        public IAspectClass AspectClass { get; }

        public IDeclaration TargetDeclaration { get; }

        public bool IsSkipped { get; private set; }

        internal void Skip() { this.IsSkipped = true; }

        public ImmutableDictionary<TemplateClass, TemplateClassInstance> TemplateInstances { get; }

        internal AspectInstance( IAspect aspect, IDeclaration declaration, AspectClass aspectClass )
        {
            this.Aspect = aspect;
            this.TargetDeclaration = declaration;
            this.AspectClass = aspectClass;

            this.TemplateInstances = ImmutableDictionary.Create<TemplateClass, TemplateClassInstance>()
                .Add( aspectClass, new TemplateClassInstance( aspect, aspectClass, declaration ) );
        }

        internal AspectInstance( IAspect aspect, IDeclaration declaration, IAspectClass aspectClass, IEnumerable<TemplateClassInstance> templateInstances )
        {
            this.Aspect = aspect;
            this.TargetDeclaration = declaration;
            this.AspectClass = aspectClass;
            this.TemplateInstances = templateInstances.ToImmutableDictionary( t => t.TemplateClass, t => t );
        }

        internal AspectInstance(
            IServiceProvider serviceProvider,
            Expression<Func<IAspect>> aspectExpression,
            IDeclaration declaration,
            AspectClass aspectClass )
        {
            var userCodeInvoker = serviceProvider.GetService<UserCodeInvoker>();

            var aspectFunc = aspectExpression.Compile();
            this.Aspect = userCodeInvoker.Invoke( () => aspectFunc() );
            this.TargetDeclaration = declaration;
            this.AspectClass = aspectClass;

            this.TemplateInstances = ImmutableDictionary.Create<TemplateClass, TemplateClassInstance>()
                .Add( aspectClass, new TemplateClassInstance( this.Aspect, aspectClass, declaration ) );
        }

        public override string ToString() => this.AspectClass.DisplayName + "@" + this.TargetDeclaration;
    }
}