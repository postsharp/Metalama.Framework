// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.CompileTime;

internal class TemplateAttributeFactory : IService
{
    private readonly AttributeDeserializer _attributeDeserializer;
    private readonly Compilation _compilation;
    private readonly INamedTypeSymbol _attributeType;

    private readonly ConcurrentDictionary<SymbolId, TemplateAttribute?> _cache = new();

    public TemplateAttributeFactory( IServiceProvider serviceProvider, Compilation compilation )
    {
        this._compilation = compilation;
        this._attributeType = this._compilation.GetTypeByMetadataName( typeof(TemplateAttribute).FullName ).AssertNotNull();
        this._attributeDeserializer = serviceProvider.GetRequiredService<CompileTimeProjectLoader>().AttributeDeserializer;
    }

    public bool TryGetTemplateAttribute(
        SymbolId memberId,
        IDiagnosticAdder diagnosticAdder,
        [NotNullWhen( true )] out TemplateAttribute? templateAttribute )
    {
        templateAttribute =
            this._cache.GetOrAdd(
                memberId,
                m =>
                {
                    _ = this.TryGetTemplateAttributeCore( m, diagnosticAdder, out var attribute );

                    return attribute;
                } );

        return templateAttribute != null;
    }

    private bool TryGetTemplateAttributeCore(
        SymbolId memberId,
        IDiagnosticAdder diagnosticAdder,
        out TemplateAttribute? templateAttribute )
    {
        var member = memberId.Resolve( this._compilation ).AssertNotNull();

        var attributeData = member
            .GetAttributes()
            .Single( a => this._compilation.HasImplicitConversion( a.AttributeClass, this._attributeType ) );

        return this._attributeDeserializer.TryCreateAttribute( attributeData, diagnosticAdder, out templateAttribute );
    }
}