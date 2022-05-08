// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Fabrics;
using Metalama.Framework.Engine.Templating;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;

namespace Metalama.Framework.Engine.Aspects
{
    /// <summary>
    /// A base class for <see cref="AspectClass"/> and <see cref="FabricTemplateClass"/>. Represents an aspect, but does not
    /// assume the class implements the <see cref="IAspect"/> semantic.
    /// </summary>
    public abstract class TemplateClass
    {
        public IServiceProvider ServiceProvider { get; }

        private readonly Dictionary<string, TemplateDriver> _templateDrivers = new( StringComparer.Ordinal );

        protected TemplateClass(
            IServiceProvider serviceProvider,
            Compilation compilation,
            INamedTypeSymbol aspectTypeSymbol,
            IDiagnosticAdder diagnosticAdder,
            TemplateClass? baseClass )
        {
            this.ServiceProvider = serviceProvider;
            this.BaseClass = baseClass;
            this.Members = this.GetMembers( compilation, aspectTypeSymbol, diagnosticAdder );
        }

        /// <summary>
        /// Gets metadata of the base aspect class.
        /// </summary>
        public TemplateClass? BaseClass { get; }

        internal ImmutableDictionary<string, TemplateClassMember> Members { get; }

        public abstract Type AspectType { get; }

        internal TemplateDriver GetTemplateDriver( IMember sourceTemplate )
        {
            var templateSymbol = sourceTemplate.GetSymbol().AssertNotNull();
            var id = templateSymbol.GetDocumentationCommentId()!;

            if ( this._templateDrivers.TryGetValue( id, out var templateDriver ) )
            {
                return templateDriver;
            }

            var templateName = TemplateNameHelper.GetCompiledTemplateName( templateSymbol );
            var compiledTemplateMethodInfo = this.AspectType.GetMethod( templateName );

            if ( compiledTemplateMethodInfo == null )
            {
                throw new AssertionFailedException( $"Could not find the compile template for {sourceTemplate}." );
            }

            templateDriver = new TemplateDriver( this.ServiceProvider, compiledTemplateMethodInfo );
            this._templateDrivers.Add( id, templateDriver );

            return templateDriver;
        }

        internal abstract CompileTimeProject? Project { get; }

        [Obfuscation( Exclude = true )] // Working around an obfuscator bug.
        public abstract string FullName { get; }

        internal bool TryGetInterfaceMember( ISymbol symbol, [NotNullWhen( true )] out TemplateClassMember? member )
            => this.Members.TryGetValue( DocumentationCommentId.CreateDeclarationId( symbol ), out member )
               && member.TemplateInfo.AttributeType == TemplateAttributeType.InterfaceMember;

        private ImmutableDictionary<string, TemplateClassMember> GetMembers( Compilation compilation, INamedTypeSymbol type, IDiagnosticAdder diagnosticAdder )
        {
            if ( compilation == null! )
            {
                // This is a test scenario where templates must not be detected.
                return ImmutableDictionary<string, TemplateClassMember>.Empty;
            }

            var symbolClassifier = this.ServiceProvider.GetRequiredService<SymbolClassificationService>().GetClassifier( compilation );

            var members = this.BaseClass?.Members.ToBuilder()
                          ?? ImmutableDictionary.CreateBuilder<string, TemplateClassMember>( StringComparer.Ordinal );

            foreach ( var memberSymbol in type.GetMembers() )
            {
                var templateInfo = symbolClassifier.GetTemplateInfo( memberSymbol ).AssertNotNull();
                var memberName = memberSymbol.Name;

                switch ( templateInfo.AttributeType )
                {
                    case TemplateAttributeType.Introduction when memberSymbol is IMethodSymbol { AssociatedSymbol: not null }:
                        // This is an accessor of an introduced event or property. We don't index them.
                        continue;

                    case TemplateAttributeType.InterfaceMember:
                        // For interface members, we don't require a unique name.
                        memberName = DocumentationCommentId.CreateDeclarationId( memberSymbol );

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
                            accessor.Parameters.Select( p => new TemplateClassMemberParameter( p.Ordinal, p.Name, false, null ) ).ToImmutableArray();

                        accessors = accessors!.Add(
                            accessor.MethodKind,
                            new TemplateClassMember(
                                accessor.Name,
                                this,
                                templateInfo!,
                                accessor,
                                accessorParameters,
                                ImmutableArray<TemplateClassMemberParameter>.Empty,
                                ImmutableDictionary<MethodKind, TemplateClassMember>.Empty ) );
                    }
                }

                switch ( memberSymbol )
                {
                    case IMethodSymbol method:
                        {
                            var parameterBuilder = ImmutableArray.CreateBuilder<TemplateClassMemberParameter>( method.Parameters.Length );
                            var allTemplateParametersCount = 0;

                            foreach ( var parameter in method.Parameters )
                            {
                                var parameterScope = symbolClassifier.GetTemplatingScope( parameter );

                                parameterBuilder.Add(
                                    new TemplateClassMemberParameter(
                                        parameter.Ordinal,
                                        parameter.Name,
                                        parameterScope == TemplatingScope.CompileTimeOnly,
                                        allTemplateParametersCount ) );

                                allTemplateParametersCount++;
                            }

                            templateParameters = parameterBuilder.MoveToImmutable();

                            var typeParameterBuilder = ImmutableArray.CreateBuilder<TemplateClassMemberParameter>( method.TypeParameters.Length );

                            foreach ( var typeParameter in method.TypeParameters )
                            {
                                var typeParameterScope = symbolClassifier.GetTemplatingScope( typeParameter );

                                typeParameterBuilder.Add(
                                    new TemplateClassMemberParameter(
                                        typeParameter.Ordinal,
                                        typeParameter.Name,
                                        typeParameterScope == TemplatingScope.CompileTimeOnly,
                                        allTemplateParametersCount ) );

                                allTemplateParametersCount++;
                            }

                            templateTypeParameters = typeParameterBuilder.MoveToImmutable();

                            break;
                        }

                    case IPropertySymbol property:
                        AddAccessor( property.GetMethod );
                        AddAccessor( property.SetMethod );

                        break;

                    case IEventSymbol @event:
                        AddAccessor( @event.AddMethod );
                        AddAccessor( @event.RemoveMethod );

                        break;
                }

                if ( memberSymbol is IMethodSymbol { MethodKind: MethodKind.PropertySet } && templateParameters.Length != 1 )
                {
                    throw new AssertionFailedException();
                }

                var aspectClassMember = new TemplateClassMember(
                    memberName,
                    this,
                    templateInfo,
                    memberSymbol,
                    templateParameters,
                    templateTypeParameters,
                    accessors );

                if ( !templateInfo.IsNone )
                {
                    if ( members.TryGetValue( memberName, out var existingMember ) && !memberSymbol.IsOverride &&
                         !existingMember.TemplateInfo.IsNone )
                    {
                        // Note we cannot get here when the member is defined in the same type because the compile-time assembly creation
                        // would have failed. The

                        // The template is already defined and we are not overwriting a template of the base class.
                        diagnosticAdder.Report(
                            GeneralDiagnosticDescriptors.TemplateWithSameNameAlreadyDefinedInBaseClass.CreateRoslynDiagnostic(
                                memberSymbol.GetDiagnosticLocation(),
                                (memberName, type.Name, existingMember.TemplateClass.AspectType.Name) ) );

                        continue;
                    }

                    // Add or replace the template.
                    members[memberName] = aspectClassMember;
                }
                else
                {
                    if ( !members.ContainsKey( memberName ) )
                    {
                        members.Add( memberName, aspectClassMember );
                    }
                }
            }

            return members.ToImmutable();
        }

        internal IEnumerable<TemplateClassMember> GetDeclarativeAdvices()
        {
            return this.Members
                .Where( m => m.Value.TemplateInfo.AttributeType == TemplateAttributeType.Introduction )
                .Select( m => m.Value )
                .OrderBy( m => m.Symbol.GetPrimarySyntaxReference()?.SyntaxTree.FilePath )
                .ThenBy( m => m.Symbol.GetPrimarySyntaxReference()?.Span.Start );
        }
    }
}