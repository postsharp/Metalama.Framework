// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace Caravela.Framework.Sdk
{

    // IMPORTANT: If you rename this class, or move it to a different namespace, you also have to update 
    // src\Compilers\Core\Portable\DiagnosticAnalyzer\AnalyzerFileReference.cs in the Caravela.Compiler repo.
    // Plus

    /// <summary>
    /// Custom attribute that, when applied to a type, exports it to the collection of compiler plug-ins. Aspect weavers are plug-ins
    /// and must be annotated with this custom attribute.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class )]
    public class CompilerPluginAttribute : Attribute
    {
    }
}
