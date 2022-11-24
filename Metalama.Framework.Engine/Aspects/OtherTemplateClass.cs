// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.Aspects;

/// <summary>
/// Represents a template class that implements <see cref="ITemplateProvider"/>.
/// </summary>
internal class OtherTemplateClass : TemplateClass
{
    public OtherTemplateClass(
        ProjectServiceProvider serviceProvider,
        CompilationContext compilationContext,
        INamedTypeSymbol typeSymbol,
        IDiagnosticAdder diagnosticAdder,
        OtherTemplateClass? baseClass,
        CompileTimeProject project )
        : base( serviceProvider, compilationContext, typeSymbol, diagnosticAdder, baseClass, typeSymbol.Name )
    {
        this.Project = project;
        this.Type = project.GetType( typeSymbol.GetReflectionName().AssertNotNull() );
    }

    public override Type Type { get; }

    internal override CompileTimeProject? Project { get; }

    public override string FullName => this.Type.FullName!;
}