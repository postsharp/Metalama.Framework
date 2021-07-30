// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Aspects
{
    [AttributeUsage( AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event )]
    internal class AbstractAttribute : Attribute { }
}