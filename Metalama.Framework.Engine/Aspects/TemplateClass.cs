// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Fabrics;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using RefKind = Microsoft.CodeAnalysis.RefKind;

namespace Metalama.Framework.Engine.Aspects
{
    /// <summary>
    /// A base class for <see cref="AspectClass"/> and <see cref="FabricTemplateClass"/>. Represents an aspect, but does not
    /// assume the class implements the <see cref="IAspect"/> semantic.
    /// </summary>
    public abstract class TemplateClass
    {
        protected ProjectServiceProvider ServiceProvider { get; }

        private readonly ConcurrentDictionary<string, TemplateDriver> _templateDrivers = new( StringComparer.Ordinal );
        private readonly TemplateClass? _baseClass;

        protected TemplateClass(
            ProjectServiceProvider serviceProvider,
            CompilationContext compilationContext,
            INamedTypeSymbol typeSymbol,
            IDiagnosticAdder diagnosticAdder,
            TemplateClass? baseClass,
            string shortName )
        {
            this.ServiceProvider = serviceProvider;
            this._baseClass = baseClass;
            this.Members = this.GetMembers( compilationContext, typeSymbol, diagnosticAdder );
            this.ShortName = shortName;

            // This condition is to work around fakes.
            if ( !typeSymbol.GetType().Assembly.IsDynamic )
            {
                this.TypeId = SerializableTypeIdProvider.GetId( typeSymbol );
            }
            else
            {
                // We have a fake!!
                this.TypeId = default;
            }
        }

        public string ShortName { get; }

        internal ImmutableDictionary<string, TemplateClassMember> Members { get; }

        protected bool HasError { get; set; }

        public SerializableTypeId TypeId { get; }

        /// <summary>
        /// Gets the reflection type for the current <see cref="TemplateClass"/>.
        /// </summary>
        internal abstract Type Type { get; }

        internal TemplateDriver GetTemplateDriver( IMember sourceTemplate )
        {
            var templateSymbol = sourceTemplate.GetSymbol().AssertNotNull();
            var id = templateSymbol.GetDocumentationCommentId()!;

            if ( this._templateDrivers.TryGetValue( id, out var templateDriver ) )
            {
                return templateDriver;
            }

            var templateName = TemplateNameHelper.GetCompiledTemplateName( templateSymbol );
            var compiledTemplateMethodInfo = this.Type.GetMethod( templateName );

            if ( compiledTemplateMethodInfo == null )
            {
                throw new AssertionFailedException( $"Could not find the compile template for {sourceTemplate}." );
            }

            templateDriver = new TemplateDriver( this.ServiceProvider, compiledTemplateMethodInfo );

            if ( this._templateDrivers.TryAdd( id, templateDriver ) )
            {
                return templateDriver;
            }
            else
            {
                // Another thread instantiated the same driver in the meantime.
                return this._templateDrivers[id];
            }
        }

        public abstract string FullName { get; }

        internal bool TryGetInterfaceMember( ISymbol symbol, [NotNullWhen( true )] out TemplateClassMember? member )
            => this.Members.TryGetValue( symbol.GetDocumentationCommentId().AssertNotNull(), out member )
               && member.TemplateInfo.AttributeType == TemplateAttributeType.InterfaceMember;

        private ImmutableDictionary<string, TemplateClassMember> GetMembers(
            CompilationContext compilationContext,
            INamedTypeSymbol type,
            IDiagnosticAdder diagnosticAdder )
        {
            var classifier = new TemplateMemberSymbolClassifier( compilationContext );

            var members = this._baseClass?.Members.ToBuilder()
                          ?? ImmutableDictionary.CreateBuilder<string, TemplateClassMember>( StringComparer.Ordinal );

            foreach ( var memberSymbol in type.GetMembers() )
            {
                var templateInfo = classifier.SymbolClassifier.GetTemplateInfo( memberSymbol ).AssertNotNull();
                var memberKey = memberSymbol.Name;

                switch ( templateInfo.AttributeType )
                {
                    case TemplateAttributeType.DeclarativeAdvice when memberSymbol is IMethodSymbol { AssociatedSymbol: not null }:
                        // This is an accessor of a template or event declarative advice. We don't index them.
                        continue;

                    case TemplateAttributeType.DeclarativeAdvice:
                    case TemplateAttributeType.InterfaceMember:
                        // For declarative advices and interface members, we don't require a unique name, so we identify the template by a special id.
                        memberKey = memberSymbol.GetDocumentationCommentId().AssertNotNull();

                        break;
                }

                var templateParameters = ImmutableArray<TemplateClassMemberParameter>.Empty;
                var templateTypeParameters = ImmutableArray<TemplateClassMemberParameter>.Empty;
                var accessors = ImmutableDictionary<MethodKind, TemplateClassMember>.Empty;

                void AddAccessor( IMethodSymbol? accessor )
                {
                    if ( accessor != null )
                    {
                        var accessorParameters =
                            accessor.Parameters.Select( p => new TemplateClassMemberParameter( p.Ordinal, p.Name, false, null ) )
                                .ToImmutableArray();

                        accessors = accessors.Add(
                            accessor.MethodKind,
                            new TemplateClassMember(
                                accessor.Name,
                                accessor.Name,
                                this,
                                templateInfo,
                                accessor.GetSymbolId(),
                                accessorParameters,
                                ImmutableArray<TemplateClassMemberParameter>.Empty,
                                ImmutableDictionary<MethodKind, TemplateClassMember>.Empty ) );
                    }
                }

                switch ( memberSymbol )
                {
                    case IMethodSymbol method:
                        {
                            // Forbid ref methods.
                            if ( method.RefKind != RefKind.None )
                            {
                                diagnosticAdder.Report(
                                    GeneralDiagnosticDescriptors.RefMembersNotSupported.CreateRoslynDiagnostic( method.GetDiagnosticLocation(), method ) );

                                this.HasError = true;
                            }

                            // Add parameters.
                            var parameterBuilder = ImmutableArray.CreateBuilder<TemplateClassMemberParameter>( method.Parameters.Length );
                            var allTemplateParametersCount = 0;

                            foreach ( var parameter in method.Parameters )
                            {
                                var isCompileTime = classifier.IsCompileTimeParameter( parameter );

                                parameterBuilder.Add(
                                    new TemplateClassMemberParameter(
                                        parameter.Ordinal,
                                        parameter.Name,
                                        isCompileTime,
                                        allTemplateParametersCount ) );

                                allTemplateParametersCount++;
                            }

                            templateParameters = parameterBuilder.MoveToImmutable();

                            // Add type parameters.
                            var typeParameterBuilder = ImmutableArray.CreateBuilder<TemplateClassMemberParameter>( method.TypeParameters.Length );

                            foreach ( var typeParameter in method.TypeParameters )
                            {
                                var isCompileTime =
                                    classifier.IsCompileTimeTemplateTypeParameter( typeParameter );

                                typeParameterBuilder.Add(
                                    new TemplateClassMemberParameter(
                                        typeParameter.Ordinal,
                                        typeParameter.Name,
                                        isCompileTime,
                                        allTemplateParametersCount ) );

                                allTemplateParametersCount++;
                            }

                            templateTypeParameters = typeParameterBuilder.MoveToImmutable();

                            break;
                        }

                    case IPropertySymbol property:
                        // Forbid ref properties.
                        if ( property.RefKind != RefKind.None )
                        {
                            diagnosticAdder.Report(
                                GeneralDiagnosticDescriptors.RefMembersNotSupported.CreateRoslynDiagnostic( property.GetDiagnosticLocation(), property ) );

                            this.HasError = true;
                        }

                        // Add accessors.
                        AddAccessor( property.GetMethod );
                        AddAccessor( property.SetMethod );

                        break;

                    // ReSharper disable once UnusedVariable
                    case IFieldSymbol field:
                        // Forbid ref fields.
#if ROSLYN_4_4_0_OR_GREATER
                        if ( field.RefKind != RefKind.None )
                        {
                            diagnosticAdder.Report(
                                GeneralDiagnosticDescriptors.RefMembersNotSupported.CreateRoslynDiagnostic( field.GetDiagnosticLocation(), field ) );

                            this.HasError = true;
                        }
#endif

                        break;

                    case IEventSymbol @event:
                        AddAccessor( @event.AddMethod );
                        AddAccessor( @event.RemoveMethod );

                        break;
                }

                if ( memberSymbol is IMethodSymbol { MethodKind: MethodKind.PropertySet } && templateParameters.Length != 1 )
                {
                    throw new AssertionFailedException(
                        $"'{memberSymbol}' is a property setter but there is {templateParameters.Length} template parameters." );
                }

                var aspectClassMember = new TemplateClassMember(
                    memberSymbol.Name,
                    memberKey,
                    this,
                    templateInfo,
                    memberSymbol.GetSymbolId(),
                    templateParameters,
                    templateTypeParameters,
                    accessors );

                if ( !templateInfo.IsNone )
                {
                    if ( members.TryGetValue( memberKey, out var existingMember ) && !memberSymbol.IsOverride &&
                         !existingMember.TemplateInfo.IsNone )
                    {
                        // Note we cannot get here when the member is defined in the same type because the compile-time assembly creation
                        // would have failed. The

                        // The template is already defined and we are not overwriting a template of the base class.
                        diagnosticAdder.Report(
                            GeneralDiagnosticDescriptors.TemplateWithSameNameAlreadyDefinedInBaseClass.CreateRoslynDiagnostic(
                                memberSymbol.GetDiagnosticLocation(),
                                (memberKey, type.Name, existingMember.TemplateClass.Type.Name) ) );

                        this.HasError = true;

                        continue;
                    }

                    // Add or replace the template.
                    members[memberKey] = aspectClassMember;
                }
                else
                {
                    if ( !members.ContainsKey( memberKey ) )
                    {
                        members.Add( memberKey, aspectClassMember );
                    }
                }
            }

            return members.ToImmutable();
        }

        internal IEnumerable<TemplateMember<IMemberOrNamedType>> GetDeclarativeAdvice( ProjectServiceProvider serviceProvider, CompilationModel compilation )
            => this.GetDeclarativeAdvice( serviceProvider, compilation.RoslynCompilation )
                .Select(
                    x => TemplateMemberFactory.Create(
                        (IMemberOrNamedType) compilation.Factory.GetDeclaration( x.Symbol ),
                        x.TemplateClassMember,
                        x.Attribute ) );

        private IEnumerable<(TemplateClassMember TemplateClassMember, ISymbol Symbol, DeclarativeAdviceAttribute Attribute)> GetDeclarativeAdvice(
            ProjectServiceProvider serviceProvider,
            Compilation compilation )
        {
            TemplateAttributeFactory? templateAttributeFactory = null;

            // We are sorting the declarative advice by symbol name and not by source order because the source is not available
            // if the aspect library is a compiled assembly.

            return this.Members
                .Where( m => m.Value.TemplateInfo.AttributeType == TemplateAttributeType.DeclarativeAdvice )
                .Select(
                    m =>
                    {
                        var symbol = m.Value.SymbolId.Resolve( compilation ).AssertNotNull();

                        return (Template: m.Value, Symbol: symbol, Syntax: symbol.GetPrimarySyntaxReference());
                    } )
                .OrderBy( m => m.Symbol, DeclarativeAdviceSymbolComparer.Instance )
                .Select( m => (m.Template, m.Symbol, ResolveAttribute( m.Template.SymbolId )) );

            DeclarativeAdviceAttribute ResolveAttribute( SymbolId templateInfoSymbolId )
            {
                templateAttributeFactory ??= serviceProvider.GetRequiredService<TemplateAttributeFactory>();

                if ( !templateAttributeFactory.TryGetTemplateAttribute( templateInfoSymbolId, NullDiagnosticAdder.Instance, out var attribute ) )
                {
                    throw new AssertionFailedException( $"Cannot get a template for '{templateInfoSymbolId}'." );
                }

                return (DeclarativeAdviceAttribute) attribute;
            }
        }
    }
}