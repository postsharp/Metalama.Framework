// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Impl.Advices;
using Metalama.Framework.Impl.Aspects;
using Metalama.Framework.Impl.CodeModel;
using Metalama.Framework.Impl.Diagnostics;
using Metalama.Framework.Impl.Pipeline;
using Metalama.Framework.Project;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Impl.Templating.MetaModel
{
    /// <summary>
    /// Encapsulates properties that are common to all constructors of <see cref="Metalama.Framework.Impl.Templating.MetaModel.MetaApi"/>.
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
        public AspectPipelineDescription PipelineDescription { get; } = ServiceProvider.GetService<AspectPipelineDescription>();
    }
}