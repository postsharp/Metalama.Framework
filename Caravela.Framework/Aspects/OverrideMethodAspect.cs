// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// A base aspect that overrides the implementation of a method.
    /// </summary>
    /// <seealso href="@overriding-methods"/>
    [AttributeUsage( AttributeTargets.Method )]
    public abstract class OverrideMethodAspect : Attribute, IAspect<IMethod>
    {
        /// <inheritdoc />
        public virtual void BuildAspect( IAspectBuilder<IMethod> builder )
        {
#if NET5_0
            var templates = new MethodTemplateSelector(
                nameof(this.OverrideMethod),
                nameof(this.OverrideAsyncMethod),
                nameof(this.OverrideEnumerableMethod),
                nameof(this.OverrideEnumeratorMethod),
                nameof(this.OverrideAsyncEnumerableMethod),
                nameof(this.OverrideAsyncEnumeratorMethod) );
#else

            var templates = new MethodTemplateSelector(
                nameof(this.OverrideMethod),
                nameof(this.OverrideAsyncMethod),
                nameof(this.OverrideEnumerableMethod),
                nameof(this.OverrideEnumeratorMethod) );

#endif

            builder.AdviceFactory.OverrideMethod( builder.TargetDeclaration, templates );
        }

        public virtual void BuildEligibility( IEligibilityBuilder<IMethod> builder ) => builder.ExceptForInheritance().MustBeNonAbstract();

        [Template]
        [Abstract]
        public virtual Task<dynamic?> OverrideAsyncMethod() => throw new NotSupportedException();

        [Template]
        [Abstract]
        public virtual IEnumerable<dynamic?> OverrideEnumerableMethod() => throw new NotSupportedException();

        [Template]
        [Abstract]
        public virtual IEnumerator<dynamic?> OverrideEnumeratorMethod() => throw new NotSupportedException();

#if NET5_0
        [Template]
        [Abstract]
        public virtual IAsyncEnumerable<dynamic?> OverrideAsyncEnumerableMethod() => throw new NotSupportedException();

        [Template]
        [Abstract]
        public virtual IAsyncEnumerable<dynamic?> OverrideAsyncEnumeratorMethod() => throw new NotSupportedException();
#endif

        public virtual void BuildAspectClass( IAspectClassBuilder builder ) { }

        /// <summary>
        /// Default template of the new method implementation.
        /// </summary>
        /// <returns></returns>
        [Template]
        public abstract dynamic? OverrideMethod();
    }
}