// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.Aspects;

internal class OtherTemplateClassFactory : TemplateClassFactory<OtherTemplateClass>
{
    public OtherTemplateClassFactory( IServiceProvider serviceProvider ) : base( serviceProvider ) { }

    protected override IEnumerable<TemplateTypeData> GetFrameworkClasses( Compilation compilation ) => Enumerable.Empty<TemplateTypeData>();

    protected override IEnumerable<string> GetTypeNames( CompileTimeProject project ) => project.OtherTemplateTypes;

    protected override bool TryCreate(
        INamedTypeSymbol templateTypeSymbol,
        Type templateReflectionType,
        OtherTemplateClass? baseClass,
        CompileTimeProject? compileTimeProject,
        IDiagnosticAdder diagnosticAdder,
        Compilation compilation,
        [NotNullWhen( true )] out OtherTemplateClass? templateClass )
    {
        templateClass = new OtherTemplateClass(
            this.ServiceProvider,
            compilation,
            templateTypeSymbol,
            diagnosticAdder,
            baseClass,
            compileTimeProject.AssertNotNull() );

        return true;
    }
}