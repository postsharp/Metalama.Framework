// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.Aspects;

/// <summary>
/// Represents a template class that implements <see cref="ITemplateProvider"/>.
/// </summary>
internal class OtherTemplateClass : TemplateClass
{
    public OtherTemplateClass(
        IServiceProvider serviceProvider,
        Compilation compilation,
        INamedTypeSymbol typeSymbol,
        IDiagnosticAdder diagnosticAdder,
        OtherTemplateClass? baseClass,
        CompileTimeProject project )
        : base( serviceProvider, compilation, typeSymbol, diagnosticAdder, baseClass, typeSymbol.Name )
    {
        this.Project = project;
        this.Type = project.GetType( typeSymbol.GetReflectionName().AssertNotNull() );
    }

    public override Type Type { get; }

    internal override CompileTimeProject? Project { get; }

    public override string FullName => this.Type.FullName!;
}