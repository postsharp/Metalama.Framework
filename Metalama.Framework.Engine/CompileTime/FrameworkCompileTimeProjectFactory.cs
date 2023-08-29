﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.CompileTime.Manifest;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Services;
using Metalama.Framework.Validation;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;

namespace Metalama.Framework.Engine.CompileTime;

internal sealed class FrameworkCompileTimeProjectFactory : IGlobalService
{
    private static readonly Assembly _frameworkAssembly = typeof(IAspect).Assembly;
    private static readonly AssemblyIdentity _frameworkAssemblyIdentity = _frameworkAssembly.GetName().ToAssemblyIdentity();

    private readonly ConcurrentDictionary<string, CompileTimeProjectManifest> _frameworkProjectManifestDictionary = new();

    private static DiagnosticManifest CreateFrameworkDiagnosticManifest()
    {
        var additionalTypes = new[] { typeof(FrameworkDiagnosticDescriptors) };
        var service = new DiagnosticDefinitionDiscoveryService();
        var diagnostics = service.GetDiagnosticDefinitions( additionalTypes ).ToImmutableArray();
        var suppressions = service.GetSuppressionDefinitions( additionalTypes ).ToImmutableArray();

        return new DiagnosticManifest( diagnostics, suppressions );
    }

    private static TemplateProjectManifest CreateFrameworkTemplateProjectManifest( IAssemblySymbol assembly )
    {
        // Create a builder.
        var builder = new TemplateProjectManifestBuilder( assembly.GlobalNamespace );

        // Index all template members.
        var typesDefiningTemplates = new[] { typeof(OverrideFieldOrPropertyAspect), typeof(OverrideMethodAspect), typeof(OverrideEventAspect) };

        foreach ( var reflectionType in typesDefiningTemplates )
        {
            var typeSymbol = assembly.GetTypeByMetadataName( reflectionType.FullName! ).AssertNotNull();

            foreach ( var member in typeSymbol.GetMembers() )
            {
                if ( member.GetAttributes().Any( a => a.AttributeClass?.Name == nameof(TemplateAttribute) ) )
                {
                    var templateInfo = new TemplateInfo( TemplateAttributeType.Template, true );
                    builder.AddOrUpdateSymbol( member, TemplatingScope.CompileTimeOnly, templateInfo );

                    // Also add to accessors.
                    void AddAccessor( IMethodSymbol? accessor )
                    {
                        if ( accessor != null )
                        {
                            builder.AddOrUpdateSymbol( accessor, TemplatingScope.CompileTimeOnly, templateInfo );

                            // Mark parameters as run-time.
                            foreach ( var parameter in accessor.Parameters )
                            {
                                builder.AddOrUpdateSymbol( parameter, TemplatingScope.RunTimeOnly );
                            }
                        }
                    }

                    switch ( member )
                    {
                        case IMethodSymbol method:
                            // Mark parameters as run-time.
                            foreach ( var parameter in method.Parameters )
                            {
                                builder.AddOrUpdateSymbol( parameter, TemplatingScope.RunTimeOnly );
                            }

                            break;

                        case IPropertySymbol property:
                            AddAccessor( property.GetMethod );
                            AddAccessor( property.SetMethod );

                            break;

                        case IEventSymbol @event:
                            AddAccessor( @event.AddMethod );
                            AddAccessor( @event.RemoveMethod );

                            break;
                    }
                }
            }
        }

        return builder.Build();
    }

    public CompileTimeProject CreateFrameworkProject( ProjectServiceProvider serviceProvider, CompileTimeDomain domain, Compilation compilation )
    {
        var assembly = compilation.SourceModule.ReferencedAssemblySymbols.First( x => x.Name == "Metalama.Framework" );

        var tfm = assembly.GetAttributes()
            .Single( attribute => attribute.AttributeClass?.Name == nameof(TargetFrameworkAttribute) )
            .ConstructorArguments
            .Single()
            .Value
            .AssertCast<string>()
            .AssertNotNull();

        var manifest = this._frameworkProjectManifestDictionary.GetOrAdd(
            tfm,
            _ => new CompileTimeProjectManifest(
                _frameworkAssemblyIdentity.ToString(),
                "",
                new[] { typeof(InternalImplementAttribute) }.SelectAsImmutableArray( t => t.FullName ),
                ImmutableArray<string>.Empty,
                ImmutableArray<string>.Empty,
                ImmutableArray<string>.Empty,
                ImmutableArray<string>.Empty,
                ImmutableArray<string>.Empty,
                CreateFrameworkTemplateProjectManifest( assembly ),
                null,
                0,
                Array.Empty<CompileTimeFileManifest>(),
                Array.Empty<CompileTimeDiagnosticManifest>() ) );

        return new CompileTimeProject(
            serviceProvider,
            domain,
            _frameworkAssemblyIdentity,
            _frameworkAssemblyIdentity,
            ImmutableArray<CompileTimeProject>.Empty,
            manifest,
            null,
            null,
            null,
            null,
            _frameworkAssembly,
            CreateFrameworkDiagnosticManifest() );
    }
}