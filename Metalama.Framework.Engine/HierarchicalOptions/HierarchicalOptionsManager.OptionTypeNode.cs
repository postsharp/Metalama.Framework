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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Metalama.Framework.Engine.HierarchicalOptions;

public sealed partial class HierarchicalOptionsManager
{
    private sealed class OptionTypeNode
    {
        private readonly HierarchicalOptionsManager _parent;
        private readonly ConcurrentDictionary<Ref<IDeclaration>, DeclarationNode> _optionsByDeclaration = new();
        private readonly EligibilityHelper _eligibilityHelper;
        private readonly Type _type;

        public HierarchicalOptionsAttribute Metadata { get; }

        public OptionTypeNode( HierarchicalOptionsManager parent, Type type, IDiagnosticAdder diagnosticAdder )
        {
            this._parent = parent;
            this._type = type;
            var invoker = parent._serviceProvider.GetRequiredService<UserCodeInvoker>();
            var context = new UserCodeExecutionContext( parent._serviceProvider, UserCodeDescription.Create( "Instantiating {0}", type ) );

            var prototype =
                invoker.Invoke( () => (IHierarchicalOptions) Activator.CreateInstance( type ).AssertNotNull(), context );

            this._eligibilityHelper = new EligibilityHelper( prototype, parent._serviceProvider, type );
            this._eligibilityHelper.PopulateRules( diagnosticAdder );
            this.Metadata = type.GetCustomAttributes<HierarchicalOptionsAttribute>().SingleOrDefault() ?? HierarchicalOptionsAttribute.Default;
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
                declarationOptions.DirectOptions =
                    MergeOptions( declarationOptions.DirectOptions, configurator.Options, HierarchicalOptionsOverrideAxis.Self, configurator.Declaration );

                declarationOptions.ResetMergedOptions();
            }
        }

        public IHierarchicalOptions? GetOptions( IDeclaration declaration )
        {
            
            var node = this.GetNodeAndComputeDirectOptions( declaration );

            if ( node?.MergedOptions != null )
            {
                // If we have a cached value, use it.
                return node.MergedOptions;
            }

            // Get options inherited from the base declaration.
            IHierarchicalOptions? baseDeclarationOptions;
            IHierarchicalOptions? containingDeclarationOptions;

            switch ( declaration )
            {
                case INamedType namedType:
                    if ( this.Metadata.InheritedByDerivedTypes && namedType.GetBaseDefinition() is { } baseType )
                    {
                        baseDeclarationOptions = this.GetOptions( baseType );
                    }
                    else
                    {
                        baseDeclarationOptions = null;
                    }

                    if ( this.Metadata.InheritedByNestedTypes && namedType.DeclaringType != null )
                    {
                        containingDeclarationOptions = this.GetOptions( namedType.DeclaringType );
                    }
                    else
                    {
                        containingDeclarationOptions = this.GetOptions( namedType.Namespace );
                    }

                    break;

                case IMember member:
                    if ( this.Metadata.InheritedByOverridingMembers && member.GetBaseDefinition() is { } baseDeclaration )
                    {
                        baseDeclarationOptions = this.GetOptions( baseDeclaration );
                    }
                    else
                    {
                        baseDeclarationOptions = null;
                    }

                    containingDeclarationOptions = this.GetOptions( member.DeclaringType );

                    break;

                case ICompilation:
                    baseDeclarationOptions = null;
                    containingDeclarationOptions = this._parent.GetDefaultOptions( this._type, declaration.Compilation.Project );

                    break;

                default:
                    baseDeclarationOptions = null;
                    containingDeclarationOptions = this.GetOptions( declaration.ContainingDeclaration.AssertNotNull() );

                    break;
            }

            // Merge all options.
            var inheritedOptions = MergeOptions(
                baseDeclarationOptions,
                containingDeclarationOptions,
                HierarchicalOptionsOverrideAxis.ContainmentOverBase,
                declaration );

            var mergedOptions = MergeOptions( inheritedOptions, node?.DirectOptions, HierarchicalOptionsOverrideAxis.DirectOverInheritance, declaration );

            // Cache the result.
            var shouldCache =
                declaration.DeclarationKind is DeclarationKind.Namespace or DeclarationKind.NamedType or DeclarationKind.Compilation ||
                (mergedOptions != null && (mergedOptions != baseDeclarationOptions || mergedOptions != containingDeclarationOptions));

            if ( shouldCache )
            {
                node ??= this.GetNodeAndComputeDirectOptions( declaration, true )!;
                node.MergedOptions = mergedOptions;
            }

            return mergedOptions;
        }

        private static IHierarchicalOptions? MergeOptions(
            IHierarchicalOptions? baseOptions,
            IHierarchicalOptions? options,
            HierarchicalOptionsOverrideAxis axis,
            IDeclaration declaration )
        {
            if ( baseOptions == null )
            {
                return options;
            }
            else if ( options == null )
            {
                return baseOptions;
            }
            else if ( ReferenceEquals( baseOptions, options ) )
            {
                return options;
            }
            else
            {
                return (IHierarchicalOptions) baseOptions.OverrideWith( options, new HierarchicalOptionsOverrideContext( axis, declaration ) );
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
            if ( !this._optionsByDeclaration.TryGetValue( declaration.ToTypedRef(), out var node ) )
            {
                if ( declaration.BelongsToCurrentProject && this._parent._externalOptionsProvider?.TryGetOptions( declaration, this._type, out var options ) == true )
                {
                    node = this.GetOrAddDeclarationNode( declaration );
                    node.DirectOptions = node.MergedOptions = options;
                }
                else if ( createNodeIfEmpty )
                {
                    node = this.GetOrAddDeclarationNode( declaration );
                }
            }

            return node;
        }

        public IEnumerable<KeyValuePair<HierarchicalOptionsKey, IHierarchicalOptions>> GetInheritableOptions( ICompilation compilation )
        {
            // We have to return the merged options of any node that has direct options. We don't return the whole cache because this cache may be incomplete.
            var optionsTypeName = this._type.FullName.AssertNotNull();

            return this._optionsByDeclaration
                    .Where( x => x.Value.DirectOptions != null )
                    .Select( x => (IDeclarationImpl) x.Key.GetTarget( compilation ) )
                    .Where( x => x.CanBeInherited )
                    .Select(
                        x => new KeyValuePair<HierarchicalOptionsKey, IHierarchicalOptions>(
                            new HierarchicalOptionsKey( optionsTypeName, x.ToSerializableId() ),
                            this.GetOptions( x ).AssertNotNull() ) )
                ;
        }
    }
}