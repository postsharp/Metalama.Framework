// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using System;

namespace Metalama.Framework.Aspects
{
    [AttributeUsage( AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property )]
    public abstract class FilterAspect : Aspect, IAspect<IParameter>, IAspect<IFieldOrPropertyOrIndexer>
    {
        public FilterDirection Direction { get; set; } = FilterDirection.Input;

        public virtual void BuildAspect( IAspectBuilder<IFieldOrPropertyOrIndexer> builder )
        {
            builder.Advice.AddFilter( builder.Target, nameof(this.Filter), this.Direction );
        }

        public virtual void BuildAspect( IAspectBuilder<IParameter> builder )
        {
            builder.Advice.AddFilter( builder.Target, nameof(this.Filter), this.Direction );
        }

        public virtual void BuildEligibility( IEligibilityBuilder<IFieldOrPropertyOrIndexer> builder )
        {
            switch ( this.Direction )
            {
                case FilterDirection.Input:
                    builder.MustBeReadable();

                    break;

                case FilterDirection.Output:
                    builder.MustBeWritable();

                    break;

                case FilterDirection.Both:
                    builder.MustSatisfyAll(
                        b => b.MustBeReadable(),
                        b => b.MustBeWritable() );

                    break;
            }
        }

        public virtual void BuildEligibility( IEligibilityBuilder<IParameter> builder )
        {
            switch ( this.Direction )
            {
                case FilterDirection.Input:
                    builder.MustBeReadable();

                    break;

                case FilterDirection.Output:
                    builder.MustBeWritable();

                    break;

                case FilterDirection.Both:
                    builder.MustSatisfyAll(
                        b => b.MustBeReadable(),
                        b => b.MustBeWritable() );

                    break;
            }
        }

        [Template]
        public abstract void Filter( dynamic? value );
    }
}