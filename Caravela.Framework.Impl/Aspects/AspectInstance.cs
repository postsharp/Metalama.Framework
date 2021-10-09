﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace Caravela.Framework.Impl.Aspects
{
    /// <summary>
    /// Represents an instance of an aspect and its target declaration.
    /// </summary>
    internal sealed class AspectInstance : IAspectInstanceInternal, IComparable<AspectInstance>
    {
        /// <summary>
        /// Gets the aspect instance.
        /// </summary>
        public IAspect Aspect { get; }

        public IAspectClass AspectClass { get; }

        public IDeclaration TargetDeclaration { get; }

        public bool IsSkipped { get; private set; }

        public ImmutableArray<IAspect> OtherInstances => ImmutableArray<IAspect>.Empty;

        public void Skip() { this.IsSkipped = true; }

        public ImmutableDictionary<TemplateClass, TemplateClassInstance> TemplateInstances { get; }
        
        public IAspectSource Source { get; }

        internal AspectInstance( IAspect aspect, IDeclaration declaration, AspectClass aspectClass, IAspectSource source )
        {
            this.Aspect = aspect;
            this.TargetDeclaration = declaration;
            this.AspectClass = aspectClass;
            this.Source = source;

            this.TemplateInstances = ImmutableDictionary.Create<TemplateClass, TemplateClassInstance>()
                .Add( aspectClass, new TemplateClassInstance( aspect, aspectClass ) );
        }

        internal AspectInstance( IAspect aspect, IDeclaration declaration, IAspectClass aspectClass, IEnumerable<TemplateClassInstance> templateInstances, IAspectSource source )
        {
            this.Aspect = aspect;
            this.TargetDeclaration = declaration;
            this.AspectClass = aspectClass;
            this.Source = source;
            this.TemplateInstances = templateInstances.ToImmutableDictionary( t => t.TemplateClass, t => t );
        }

        internal AspectInstance(
            IServiceProvider serviceProvider,
            Expression<Func<IAspect>> aspectExpression,
            IDeclaration declaration,
            AspectClass aspectClass,
            IAspectSource source )
        {
            var userCodeInvoker = serviceProvider.GetService<UserCodeInvoker>();

            var aspectFunc = aspectExpression.Compile();
            this.Aspect = userCodeInvoker.Invoke( () => aspectFunc() );
            this.TargetDeclaration = declaration;
            this.AspectClass = aspectClass;
            this.Source = source;

            this.TemplateInstances = ImmutableDictionary.Create<TemplateClass, TemplateClassInstance>()
                .Add( aspectClass, new TemplateClassInstance( this.Aspect, aspectClass ) );
        }

        public override string ToString() => this.AspectClass.DisplayName + "@" + this.TargetDeclaration;

        public int CompareTo( AspectInstance? other ) => this.Source.Priority.CompareTo( other.AssertNotNull().Source.Priority );

        
    }
}