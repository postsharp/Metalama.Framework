// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Metalama.Framework.Engine.HierarchicalOptions;

internal sealed class CompilationConfiguratorSource : IConfiguratorSource
{
    private readonly IUserCodeAttributeDeserializer _attributeDeserializer;

    private readonly ConcurrentDictionary<Type, Func<IHierarchicalOptionsProvider, IHierarchicalOptions>> _toOptionsMethods = new();
    private readonly ProjectSpecificCompileTimeTypeResolver _typeResolver;

    public CompilationConfiguratorSource( ProjectServiceProvider serviceProvider )
    {
        this._attributeDeserializer = serviceProvider.GetRequiredService<IUserCodeAttributeDeserializer>();
        this._typeResolver = serviceProvider.GetRequiredService<ProjectSpecificCompileTimeTypeResolver>();
    }

    private Func<IHierarchicalOptionsProvider, IHierarchicalOptions> GetToOptionsMethod( Type type )
        => this._toOptionsMethods.GetOrAdd( type, GetToOptionsMethodCore );

    private static Func<IHierarchicalOptionsProvider, IHierarchicalOptions> GetToOptionsMethodCore( Type type )
    {
        var parameter = Expression.Parameter( typeof(IHierarchicalOptionsProvider) );
        var interfaceType = typeof(IHierarchicalOptionsProvider<>).MakeGenericType( type );
        var cast = Expression.Convert( parameter, interfaceType );

        var methodCall = Expression.Call(
            cast,
            interfaceType.GetMethod( nameof(IHierarchicalOptionsProvider<IHierarchicalOptions>.GetOptions) ).AssertNotNull() );

        return Expression.Lambda<Func<IHierarchicalOptionsProvider, IHierarchicalOptions>>( methodCall, parameter ).Compile();
    }

    public IEnumerable<Configurator> GetConfigurators( CompilationModel compilation, IDiagnosticAdder diagnosticAdder )
    {
        var genericIHierarchicalOptionsAttribute = compilation.Factory.GetTypeByReflectionType( typeof(IHierarchicalOptionsProvider<>) );
        var aspectType = compilation.Factory.GetTypeByReflectionType( typeof(IAspect) );

        foreach ( var attributeType in compilation.GetDerivedTypes(
                     (INamedType) compilation.Factory.GetTypeByReflectionType( typeof(IHierarchicalOptionsProvider) ) ) )
        {
            if ( attributeType.Is( aspectType ) )
            {
                // Aspects can implement IHierarchicalOptionsProvider but their options are exposed on IAspectInstance.GetOptions
                // or IAspectBuilder.Options and are not handled by the current facility. Because we want options defined
                // on aspects to have absolute priority.
                continue;
            }

            var optionTypes =
                attributeType.AllImplementedInterfaces.Where( i => i.TypeDefinition == genericIHierarchicalOptionsAttribute )
                    .Select( x => (INamedType) x.TypeArguments[0] )
                    .Select( x => this._typeResolver.GetCompileTimeType( x.GetSymbol().AssertNotNull(), false ).AssertNotNull() )
                    .ToReadOnlyList();

            foreach ( var attribute in compilation.GetAllAttributesOfType( attributeType ) )
            {
                if ( !this._attributeDeserializer.TryCreateAttribute( attribute.GetAttributeData(), diagnosticAdder, out var deserializedAttribute ) )
                {
                    continue;
                }

                var optionsAttribute = (IHierarchicalOptionsProvider) deserializedAttribute;

                foreach ( var optionType in optionTypes )
                {
                    var options = this.GetToOptionsMethod( optionType ).Invoke( optionsAttribute );

                    yield return new Configurator( attribute.ContainingDeclaration, options );
                }
            }
        }
    }
}