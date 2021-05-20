// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;
using System;

namespace Caravela.Framework.Advices
{
    [AttributeUsage( AttributeTargets.Method, Inherited = true )]
    public class OverrideFieldOrPropertySetTemplateAttribute : TemplateAttribute { }
}