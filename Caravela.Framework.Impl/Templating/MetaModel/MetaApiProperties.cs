// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Templating.MetaModel
{
    /// <summary>
    /// Encapsulates properties that are common to all constructors of <see cref="Caravela.Framework.Impl.Templating.MetaModel.MetaApi"/>.
    /// </summary>
    internal record MetaApiProperties(
        UserDiagnosticSink Diagnostics,
        ISymbol TemplateSymbol,
        IReadOnlyDictionary<string, object?> Tags,
        AspectLayerId AspectLayerId );
}