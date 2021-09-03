// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        private bool _useEnumerableTemplateForAnyEnumerable;
        private bool _useAsyncTemplateForAnyAwaitable;
        private bool _buildAspectCalled;

        /// <inheritdoc />
        public virtual void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            this.EnsureBuildAspectNotCalled();

#if NET5_0
            var templates = new MethodTemplateSelector(
                nameof(this.OverrideMethod),
                nameof(this.OverrideAsyncMethod),
                nameof(this.OverrideEnumerableMethod),
                nameof(this.OverrideEnumeratorMethod),
                nameof(this.OverrideAsyncEnumerableMethod),
                nameof(this.OverrideAsyncEnumeratorMethod),
                this.UseAsyncTemplateForAnyAwaitable,
                this.UseEnumerableTemplateForAnyEnumerable );
#else
            var templates = new MethodTemplateSelector(
                nameof(this.OverrideMethod),
                nameof(this.OverrideAsyncMethod),
                nameof(this.OverrideEnumerableMethod),
                nameof(this.OverrideEnumeratorMethod),
                null,
                null,
                this.UseAsyncTemplateForAnyAwaitable,
                this.UseEnumerableTemplateForAnyEnumerable );
#endif

            this._buildAspectCalled = true;
            builder.AdviceFactory.OverrideMethod( builder.Target, templates );
        }

        private void EnsureBuildAspectNotCalled( [CallerMemberName] string? caller = null )
        {
            if ( this._buildAspectCalled )
            {
                throw new InvalidOperationException( $"Cannot access {caller} because the {nameof(this.BuildAspect)} method has already been invoked." );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="OverrideAsyncMethod"/> template must be applied to all methods returning an awaitable
        /// type (including <c>IAsyncEnumerable</c> and <c>IAsyncEnumerator</c>), instead of only to methods that have the <c>async</c> modifier.
        /// </summary>
        protected bool UseEnumerableTemplateForAnyEnumerable
        {
            get => this._useEnumerableTemplateForAnyEnumerable;
            set
            {
                this.EnsureBuildAspectNotCalled();
                this._useEnumerableTemplateForAnyEnumerable = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="OverrideEnumerableMethod"/>, <see cref="OverrideEnumeratorMethod"/>,
        /// <c>OverrideAsyncEnumerableMethod"</c> or  <c>OverrideAsyncEnumeratorMethod"</c> template must be applied to all methods returning
        /// a compatible return type, instead of only to methods using the <c>yield</c> statement.
        /// </summary>
        protected bool UseAsyncTemplateForAnyAwaitable
        {
            get => this._useAsyncTemplateForAnyAwaitable;
            set
            {
                this.EnsureBuildAspectNotCalled();
                this._useAsyncTemplateForAnyAwaitable = value;
            }
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
        public virtual IAsyncEnumerator<dynamic?> OverrideAsyncEnumeratorMethod() => throw new NotSupportedException();
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