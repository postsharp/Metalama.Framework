// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Services;
using Microsoft.CodeAnalysis;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.CompileTime;

internal sealed class TemplateAttributeFactory : IProjectService
{
    private readonly IAttributeDeserializer _attributeDeserializer;
    private readonly Compilation _compilation;
    private readonly INamedTypeSymbol _adviceAttributeType;

    private readonly ConcurrentDictionary<SerializableDeclarationId, IAdviceAttribute?> _cacheById = new();
    private readonly ConcurrentDictionary<ISymbol, IAdviceAttribute?> _cacheBySymbol = new();

    public TemplateAttributeFactory( ProjectServiceProvider serviceProvider, Compilation compilation )
    {
        this._compilation = compilation;
        this._adviceAttributeType = this._compilation.GetTypeByMetadataName( typeof(IAdviceAttribute).FullName.AssertNotNull() ).AssertNotNull();
        this._attributeDeserializer = serviceProvider.GetRequiredService<IUserCodeAttributeDeserializer>();
    }

    public bool TryGetTemplateAttribute(
        SerializableDeclarationId memberId,
        IDiagnosticAdder diagnosticAdder,
        [NotNullWhen( true )] out IAdviceAttribute? adviceAttribute )
    {
        if ( this._cacheById.TryGetValue( memberId, out adviceAttribute ) )
        {
            return true;
        }

        adviceAttribute =
            this._cacheById.GetOrAdd(
                memberId,
                m =>
                {
                    _ = this.TryGetTemplateAttributeById( m, diagnosticAdder, out var attribute );

                    return attribute;
                } );

        return adviceAttribute != null;
    }

    private bool TryGetTemplateAttributeById(
        SerializableDeclarationId memberId,
        IDiagnosticAdder diagnosticAdder,
        out IAdviceAttribute? adviceAttribute )
    {
        var member = memberId.ResolveToSymbol( this._compilation ).AssertNotNull();

        if ( this._cacheBySymbol.TryGetValue( member, out adviceAttribute ) )
        {
            return true;
        }

        adviceAttribute = this._cacheBySymbol.GetOrAdd(
            member,
            m =>
            {
                _ = this.TryGetTemplateAttributeBySymbol( m, diagnosticAdder, out var attribute );

                return attribute;
            } );

        return adviceAttribute != null;
    }

    private bool TryGetTemplateAttributeBySymbol(
        ISymbol member,
        IDiagnosticAdder diagnosticAdder,
        out IAdviceAttribute? adviceAttribute )
    {
        var attributeData = member
            .GetAttributes()
            .SingleOrDefault( a => this._compilation.HasImplicitConversion( a.AttributeClass, this._adviceAttributeType ) );

        if ( attributeData == null )
        {
            // We if we don't have a custom attribute here, we need to look at parent declarations.

            if ( member.IsOverride )
            {
                var overriddenMember = member switch
                {
                    IMethodSymbol method => (ISymbol?) method.OverriddenMethod,
                    IPropertySymbol property => property.OverriddenProperty,
                    IEventSymbol @event => @event.OverriddenEvent,
                    _ => throw new AssertionFailedException( $"Unexpected symbol: '{member}'." )
                };

                if ( overriddenMember != null )
                {
                    return this.TryGetTemplateAttributeBySymbol( overriddenMember, diagnosticAdder, out adviceAttribute );
                }
                else
                {
                    throw new AssertionFailedException( $"Cannot find the TemplateAttribute for '{member}'." );
                }
            }
            else if ( member is IMethodSymbol { AssociatedSymbol: { } associatedSymbol } )
            {
                return this.TryGetTemplateAttributeBySymbol( associatedSymbol, diagnosticAdder, out adviceAttribute );
            }
        }

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