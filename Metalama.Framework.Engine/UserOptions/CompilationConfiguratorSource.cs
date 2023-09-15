// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

namespace Metalama.Framework.Engine.UserOptions;

internal class CompilationConfiguratorSource : IConfiguratorSource
{
    private readonly ISystemAttributeDeserializer _attributeDeserializer;

    private readonly ConcurrentDictionary<Type, Func<IAspectOptionsAttribute, AspectOptions>> _toOptionsMethods = new();

    public CompilationConfiguratorSource( ProjectServiceProvider serviceProvider, CompilationModel compilationModel )
    {
        this.OptionTypes = compilationModel.GetDerivedTypes( (INamedType) compilationModel.Factory.GetTypeByReflectionType( typeof(IAspectOptionsAttribute) ) )
            .Select( t => t.GetFullMetadataName() )
            .ToImmutableArray();

        this._attributeDeserializer = serviceProvider.GetRequiredService<ISystemAttributeDeserializer>();
    }

    public ImmutableArray<string> OptionTypes { get; }

    private Func<IAspectOptionsAttribute, AspectOptions> GetToOptionsMethod( Type type ) => this._toOptionsMethods.GetOrAdd( type, GetToOptionsMethodCore );

    private static Func<IAspectOptionsAttribute, AspectOptions> GetToOptionsMethodCore( Type type )
    {
        var parameter = Expression.Parameter( typeof(IAspectOptionsAttribute) );
        var interfaceType = typeof(IAspectOptionsAttribute<>).MakeGenericType( type );
        var cast = Expression.Convert( parameter, interfaceType );
        var methodCall = Expression.Call( cast, interfaceType.GetMethod( nameof(IAspectOptionsAttribute<AspectOptions>.ToOptions) ).AssertNotNull() );

        return Expression.Lambda<Func<IAspectOptionsAttribute, AspectOptions>>( methodCall ).Compile();
    }

    public IEnumerable<UserOptionsConfigurator> GetConfigurators( string optionsTypeName, CompilationModel compilation, IDiagnosticAdder diagnosticAdder )
    {
        var optionsType = compilation.Factory.GetTypeByReflectionName( optionsTypeName );
        
        foreach ( var attribute in compilation.GetAllAttributesOfType( optionsType ) )
        {
            if ( !this._attributeDeserializer.TryCreateAttribute( attribute.GetAttributeData(), diagnosticAdder, out var deserializedAttribute ) )
            {
                continue;
            }

            var optionsAttribute = (IAspectOptionsAttribute) deserializedAttribute;

            foreach ( var optionType in optionsAttribute.SupportedOptionTypes )
            {
                var options = this.GetToOptionsMethod( optionType ).Invoke( optionsAttribute );

                yield return new UserOptionsConfigurator( attribute.ContainingDeclaration, options );
            }
        }
    }
}