// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.ServiceProvider;
using Caravela.Framework.Impl.Utilities;
using System;
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

        /// <summary>
        /// Gets the declaration to which the aspect is applied.
        /// </summary>
        public IDeclaration TargetDeclaration { get; }

        public IAspectClassImpl AspectClass { get; }

        public bool IsSkipped { get; private set; }

        internal void Skip() { this.IsSkipped = true; }

        IAspectClass IAspectInstance.AspectClass => this.AspectClass;

        internal AspectInstance( IAspect aspect, IDeclaration declaration, IAspectClassImpl aspectClass )
        {
            this.Aspect = aspect;
            this.TargetDeclaration = declaration;
            this.AspectClass = aspectClass;
        }

        internal AspectInstance(
            IServiceProvider serviceProvider,
            Expression<Func<IAspect>> aspectExpression,
            IDeclaration declaration,
            IAspectClassImpl aspectClass )
        {
            var userCodeInvoker = serviceProvider.GetService<UserCodeInvoker>();

            var aspectFunc = aspectExpression.Compile();
            this.Aspect = userCodeInvoker.Invoke( () => aspectFunc() );
            this.TargetDeclaration = declaration;
            this.AspectClass = aspectClass;
        }

        public override string ToString() => this.AspectClass.DisplayName + "@" + this.TargetDeclaration;
    }
}