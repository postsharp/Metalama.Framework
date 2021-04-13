// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Project;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// The base class for all custom attributes that mark a declaration as a template.
    /// </summary>
    [AttributeUsage( AttributeTargets.All )]
    public abstract class TemplateAttribute : CompileTimeOnlyAttribute
    {
        protected TemplateAttribute()
        {
        }
    }
}