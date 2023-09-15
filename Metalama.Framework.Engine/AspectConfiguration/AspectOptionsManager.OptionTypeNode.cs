// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities.UserCode;
using Metalama.Framework.Options;
using System;
using System.Collections.Concurrent;

namespace Metalama.Framework.Engine.AspectConfiguration;

public partial class AspectOptionsManager
{
    private sealed class OptionTypeNode
    {
        private readonly AspectOptionsManager _parent;
        private readonly ConcurrentDictionary<Ref<IDeclaration>, DeclarationNode> _optionsByDeclaration = new();
        private readonly EligibilityHelper _eligibilityHelper;
        private readonly Type _type;

        public OptionTypeNode( AspectOptionsManager parent, Type type, IDiagnosticAdder diagnosticAdder )
        {
            this._parent = parent;
            this._type = type;
            var invoker = parent._serviceProvider.GetRequiredService<UserCodeInvoker>();
            var context = new UserCodeExecutionContext( parent._serviceProvider, UserCodeDescription.Create( "Instantiating {0}", type ) );

            var prototype =
                invoker.Invoke( () => (Framework.Options.AspectOptions) Activator.CreateInstance( type ).AssertNotNull(), context );

            this._eligibilityHelper = new EligibilityHelper( prototype, parent._serviceProvider, type );
            this._eligibilityHelper.PopulateRules( diagnosticAdder );
        }

        public void AddConfigurator( Configurator configurator, IDiagnosticAdder diagnosticAdder )
        {
            // ReSharper disable once InconsistentlySynchronizedField
            var declarationOptions = this.GetOrAddDeclarationNode( configurator.Declaration );

            // Check the eligibility of the options on the target declaration.
            var eligibility = this._eligibilityHelper.GetEligibility( configurator.Declaration, true );

            if ( eligibility == EligibleScenarios.None )
            {
                var justification = this._eligibilityHelper.GetIneligibilityJustification(
                    EligibleScenarios.Default | EligibleScenarios.Inheritance,
                    new DescribedObject<IDeclaration>( configurator.Declaration ) );

                diagnosticAdder.Report(
                    GeneralDiagnosticDescriptors.OptionNotEligibleOnTarget.CreateRoslynDiagnostic(
                        configurator.Declaration.GetDiagnosticLocation(),
                        (this._type.Name, configurator.Declaration.DeclarationKind, configurator.Declaration, justification!) ) );

                return;
            }

            lock ( declarationOptions.Sync )
            {
                declarationOptions.DirectOptions ??=
                    declarationOptions.DirectOptions?.OverrideWith(
                        configurator.Options,
                        new AspectOptionsOverrideContext( AspectOptionsOverrideAxis.Self ) )
                    ?? configurator.Options;

                declarationOptions.ResetMergedOptions();
            }
        }

        public T? GetOptions<T>( IDeclaration declaration )
            where T : Framework.Options.AspectOptions, new()
        {
            var node = this.GetNodeAndComputeDirectOptions( declaration );

            if ( node?.MergedOptions != null )
            {
                // If we have a cached value, use it.
                return (T) node.MergedOptions;
            }

            // Get options inherited from the base declaration.
            T? baseDeclarationOptions;

            if ( declaration is IMemberOrNamedType memberOrNamedType )
            {
                var baseDeclaration = memberOrNamedType.GetBaseDefinition();
                baseDeclarationOptions = baseDeclaration != null ? this.GetOptions<T>( baseDeclaration ) : null;
            }
            else if ( declaration.DeclarationKind == DeclarationKind.Compilation )
            {
                // For the compilation object, we take the default options initialized with project properties.
                baseDeclarationOptions = this._parent.GetDefaultOptions<T>( declaration.Compilation );
            }
            else
            {
                baseDeclarationOptions = null;
            }

            // Get options inherited from containing declaration.
            T? containingDeclarationOptions;

            var containingDeclaration = declaration switch
            {
                INamedType namedType => (IDeclaration?) namedType.DeclaringType ?? namedType.Namespace,
                _ => declaration.ContainingDeclaration
            };

            if ( containingDeclaration != null )
            {
                containingDeclarationOptions = this.GetOptions<T>( containingDeclaration );
            }
            else
            {
                containingDeclarationOptions = null;
            }

            // Merge all options.
            var mergedOptions =
                MergeOptions(
                    MergeOptions( baseDeclarationOptions, containingDeclarationOptions, AspectOptionsOverrideAxis.ContainmentOverBase ),
                    node?.DirectOptions,
                    AspectOptionsOverrideAxis.DirectOverInheritance );

            // Cache the result.
            var shouldCache =
                (declaration.DeclarationKind is DeclarationKind.Namespace or DeclarationKind.NamedType or DeclarationKind.Compilation) ||
                (mergedOptions != null && (mergedOptions != baseDeclarationOptions || mergedOptions != containingDeclarationOptions));

            if ( shouldCache )
            {
                node ??= this.GetNodeAndComputeDirectOptions( declaration, true )!;
                node.MergedOptions = mergedOptions;
            }

            return (T?) mergedOptions;
        }

        private static T? MergeOptions<T>( T? baseOptions, T? options, AspectOptionsOverrideAxis axis )
            where T : Framework.Options.AspectOptions
        {
            if ( baseOptions == null )
            {
                return options;
            }
            else if ( options == null )
            {
                return baseOptions;
            }
            else
            {
                return (T) baseOptions.OverrideWith( options, new AspectOptionsOverrideContext( axis ) );
            }
        }

        private DeclarationNode GetOrAddDeclarationNode( IDeclaration declaration )
        {
            return this._optionsByDeclaration.GetOrAdd(
                declaration.ToTypedRef(),
                _ =>
                {
                    var node = new DeclarationNode();

                    // Note that in case of race we may wire a node that will be unused,
                    // but this should not affect the consistency of the data structure.
                    this.WireNodeToParents( declaration, node );

                    return node;
                } );
        }

        private void WireNodeToParents( IDeclaration declaration, DeclarationNode node )
        {
            var baseDeclaration = (declaration as IMemberOrNamedType)?.GetBaseDefinition();

            if ( baseDeclaration != null )
            {
                var baseNode = this.GetOrAddDeclarationNode( baseDeclaration );
                baseNode.AddChildNode( node );
            }

            var containingDeclaration = declaration.ContainingDeclaration;

            if ( containingDeclaration != null )
            {
                var containingNode = this.GetOrAddDeclarationNode( containingDeclaration );
                containingNode.AddChildNode( node );
            }
        }

        private DeclarationNode? GetNodeAndComputeDirectOptions(
            IDeclaration declaration,
            bool createNodeIfEmpty = false )
        {
            // ReSharper disable once InconsistentlySynchronizedField
            if ( !this._optionsByDeclaration.TryGetValue( declaration.ToTypedRef(), out var optionsOnDeclaration ) && createNodeIfEmpty )
            {
                optionsOnDeclaration = this.GetOrAddDeclarationNode( declaration );
            }

            return optionsOnDeclaration;
        }
    }
}