﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;

#pragma warning disable CS0162 // Unreacheable code.

namespace CodeCoverage
{
    public class NonInlineablePropertyAspect : OverrideFieldOrPropertyAspect
    {
        public override dynamic OverrideProperty
        {
            get
            {
                if ( true )
                {
                    return meta.Proceed();
                }
                else
                {
                    return meta.Proceed();
                }
            }

            set
            {
                if ( true )
                {
                    meta.Proceed();
                }
                else
                {
                    meta.Proceed();
                }
            }
        }
    }

}