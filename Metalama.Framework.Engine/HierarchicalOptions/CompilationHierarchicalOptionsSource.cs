// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Options;
using System.Collections.Generic;
using System.Linq;
using Attribute = System.Attribute;

namespace Metalama.Framework.Engine.HierarchicalOptions;

internal sealed class CompilationHierarchicalOptionsSource : IHierarchicalOptionsSource
{
    private readonly ProjectServiceProvider _serviceProvider;
    private readonly IUserCodeAttributeDeserializer _attributeDeserializer;
    private readonly UserCodeInvoker _invoker;
    private readonly ProjectSpecificCompileTimeTypeResolver _typeResolver;

    public CompilationHierarchicalOptionsSource( ProjectServiceProvider serviceProvider )
    {
        this._serviceProvider = serviceProvider;
        this._attributeDeserializer = serviceProvider.GetRequiredService<IUserCodeAttributeDeserializer>();
        this._invoker = serviceProvider.GetRequiredService<UserCodeInvoker>();
        this._typeResolver = this._serviceProvider.GetRequiredService<ProjectSpecificCompileTimeTypeResolver>();
    }

    public IEnumerable<HierarchicalOptionsInstance> GetOptions( CompilationModel compilation, IUserDiagnosticSink diagnosticSink )
    {
        // For each known options type, compute its default value for this compilation.
        foreach ( var optionsType in compilation.GetDerivedTypes(
                     (INamedType) compilation.Factory.GetTypeByReflectionType( typeof(IHierarchicalOptions) ),
                     DerivedTypesOptions.IncludingExternalTypesDangerous ) )
        {
            if ( optionsType.IsAbstract )
            {
                continue;
            }

            var optionsSystemType = this._typeResolver.GetCompileTimeType( optionsType.GetSymbol().AssertNotNull(), false ).AssertNotNull();

            var (defaultOptions, _) = HierarchicalOptionsManager.ComputeDefaultOptions( optionsSystemType, compilation, this._invoker, this._serviceProvider, diagnosticSink );

            yield return new HierarchicalOptionsInstance( compilation.DeclaringAssembly, defaultOptions );
        }

        var aspectType = compilation.Factory.GetTypeByReflectionType( typeof(IAspect) );
        var systemAttributeType = compilation.Factory.GetTypeByReflectionType( typeof(Attribute) );

        foreach ( var attributeType in compilation.GetDerivedTypes(
                     (INamedType) compilation.Factory.GetTypeByReflectionType( typeof(IHierarchicalOptionsProvider) ),
                     DerivedTypesOptions.IncludingExternalTypesDangerous ) )
        {
            if ( attributeType.Is( aspectType ) )
            {
                // Aspects can implement IHierarchicalOptionsProvider but their options are exposed on IAspectInstance.GetOptions
                // or IAspectBuilder.Options and are not handled by the current facility. Because we want options defined
                // on aspects to have absolute priority.
                continue;
            }
            else if ( !attributeType.Is( systemAttributeType ) )
            {
                continue;
            }

            foreach ( var attribute in compilation.GetAllAttributesOfType( attributeType ) )
            {
                if ( !this._attributeDeserializer.TryCreateAttribute( attribute.GetAttributeData(), diagnosticSink, out var deserializedAttribute ) )
                {
                    continue;
                }

                var optionsAttribute = (IHierarchicalOptionsProvider) deserializedAttribute;

                var invokerContext = new UserCodeExecutionContext(
                    this._serviceProvider,
                    UserCodeDescription.Create( "executing GetOptions() for '{0}' applied to '{1}'", attributeType.Name, attribute.ContainingDeclaration ),
                    targetDeclaration: attribute.ContainingDeclaration );

                var providerContext = new OptionsProviderContext(
                    attribute.ContainingDeclaration,
                    new ScopedDiagnosticSink( diagnosticSink, new ProvideOptionsDiagnosticSource( attribute ), attribute, attribute.ContainingDeclaration ) );

                var optionList = this._invoker.Invoke( () => optionsAttribute.GetOptions( providerContext ).ToReadOnlyList(), invokerContext );

                foreach ( var options in optionList )
                {
                    yield return new HierarchicalOptionsInstance( attribute.ContainingDeclaration, options );
                }
            }
        }
    }

    private sealed class ProvideOptionsDiagnosticSource : IDiagnosticSource
    {
        private readonly IAttribute _attribute;

        public ProvideOptionsDiagnosticSource( IAttribute attribute )
        {
            this._attribute = attribute;
        }

        public string DiagnosticSourceDescription
            => $"executing '{this._attribute.Type.Name}.{nameof(IHierarchicalOptionsProvider.GetOptions)}' for '{this._attribute.ContainingDeclaration}'";
    }
}