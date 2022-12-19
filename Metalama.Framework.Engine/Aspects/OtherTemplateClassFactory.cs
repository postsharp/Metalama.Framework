// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.Aspects;

internal sealed class OtherTemplateClassFactory : TemplateClassFactory<OtherTemplateClass>
{
    protected override IEnumerable<TemplateTypeData> GetFrameworkClasses( Compilation compilation ) => Enumerable.Empty<TemplateTypeData>();

    protected override IEnumerable<string> GetTypeNames( CompileTimeProject project ) => project.OtherTemplateTypes;

    protected override bool TryCreate(
        ProjectServiceProvider serviceProvider,
        INamedTypeSymbol templateTypeSymbol,
        Type templateReflectionType,
        OtherTemplateClass? baseClass,
        CompileTimeProject? compileTimeProject,
        IDiagnosticAdder diagnosticAdder,
        CompilationContext compilationContext,
        [NotNullWhen( true )] out OtherTemplateClass? templateClass )
    {
        templateClass = new OtherTemplateClass(
            serviceProvider,
            compilationContext,
            templateTypeSymbol,
            diagnosticAdder,
            baseClass,
            compileTimeProject.AssertNotNull() );

        return true;
    }
}