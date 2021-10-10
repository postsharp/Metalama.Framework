// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using System;

namespace Caravela.Framework.Aspects
{
    [AttributeUsage( AttributeTargets.Field )]
    public abstract class FieldAspect : Aspect, IAspect<IField>
    {
        public virtual void BuildAspect( IAspectBuilder<IField> builder ) { }

        public virtual void BuildEligibility( IEligibilityBuilder<IField> builder ) { }
    }
}