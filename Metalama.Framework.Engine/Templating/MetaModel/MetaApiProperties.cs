// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advices;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Project;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Templating.MetaModel
{
    /// <summary>
    /// Encapsulates properties that are common to all constructors of <see cref="Metalama.Framework.Engine.Templating.MetaModel.MetaApi"/>.
    /// </summary>
    internal record MetaApiProperties(
        UserDiagnosticSink Diagnostics,
        TemplateMember<IMemberOrNamedType> Template,
        IReadOnlyDictionary<string, object?> Tags,
        AspectLayerId AspectLayerId,
        SyntaxGenerationContext SyntaxGenerationContext,
        IAspectInstance AspectInstance,
        IServiceProvider ServiceProvider )
    {
        public AspectPipelineDescription PipelineDescription { get; } = ServiceProvider.GetRequiredService<AspectPipelineDescription>();
    }
}