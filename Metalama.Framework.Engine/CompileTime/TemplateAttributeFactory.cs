﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
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
    private readonly INamedTypeSymbol _adviceAttributeType;

    private readonly ConcurrentDictionary<SymbolId, IAdviceAttribute?> _cache = new();

    public TemplateAttributeFactory( IServiceProvider serviceProvider, Compilation compilation )
    {
        this._compilation = compilation;
        this._adviceAttributeType = this._compilation.GetTypeByMetadataName( typeof(IAdviceAttribute).FullName ).AssertNotNull();
        this._attributeDeserializer = serviceProvider.GetRequiredService<CompileTimeProjectLoader>().AttributeDeserializer;
    }

    public bool TryGetTemplateAttribute(
        SymbolId memberId,
        IDiagnosticAdder diagnosticAdder,
        [NotNullWhen( true )] out IAdviceAttribute? adviceAttribute )
    {
        adviceAttribute =
            this._cache.GetOrAdd(
                memberId,
                m =>
                {
                    _ = this.TryGetTemplateAttributeCore( m, diagnosticAdder, out var attribute );

                    return attribute;
                } );

        return adviceAttribute != null;
    }

    private bool TryGetTemplateAttributeCore(
        SymbolId memberId,
        IDiagnosticAdder diagnosticAdder,
        out IAdviceAttribute? adviceAttribute )
    {
        var member = memberId.Resolve( this._compilation ).AssertNotNull();

        var attributeData = member
            .GetAttributes()
            .Single( a => this._compilation.HasImplicitConversion( a.AttributeClass, this._adviceAttributeType ) );

        if ( !this._attributeDeserializer.TryCreateAttribute( attributeData, diagnosticAdder, out var attribute ) )
        {
            adviceAttribute = null;

            return false;
        }
        else
        {
            adviceAttribute = (IAdviceAttribute) attribute;

            return true;
        }
    }
}