// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Aspects
{
    public abstract class Aspect : Attribute, IAspect
    {
        public virtual void BuildAspectClass( IAspectClassBuilder builder ) { }
    }
}