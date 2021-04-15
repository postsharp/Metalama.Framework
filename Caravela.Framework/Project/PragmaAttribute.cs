// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Project
{
    /// <summary>
    /// Custom attribute that means that the method must be processed syntaxically by the template compiler instead of being classically
    /// compile-time or run-time. Used to add trivias to the transformed code.
    /// </summary>
    [AttributeUsage( AttributeTargets.Method )]
    public class PragmaAttribute : TemplateKeywordAttribute { }
}