// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.Advices;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Caravela.Framework.Impl
{
    internal class AdviceFactory : IAdviceFactory
    {
        private readonly CompilationModel _compilation;
        private readonly INamedType _aspectType;
        private readonly AspectInstance _aspect;

        private readonly List<IAdvice> _advices = new();

        internal IReadOnlyList<IAdvice> Advices => this._advices;

        public AdviceFactory( CompilationModel compilation, INamedType aspectType, AspectInstance aspect )
        {
            this._aspectType = aspectType;
            this._aspect = aspect;
            this._compilation = compilation;
        }

        private IMethod? GetTemplateMethod( string methodName, Type expectedAttributeType, string adviceName, [DoesNotReturnIf(true)] bool throwIfMissing = true )
        {
            // We do the search against the Roslyn compilation because it is cheaper.

            var methods = this._aspectType.GetSymbol().GetMembers( methodName ).OfType<IMethodSymbol>().ToList();
            var expectedAttributeTypeSymbol = this._compilation.ReflectionMapper.GetTypeSymbol( expectedAttributeType );

            if ( methods.Count != 1 )
            {
                throw GeneralDiagnosticDescriptors.AspectMustHaveExactlyOneTemplateMethod.CreateException( (this._aspectType, methodName) );
            }

            var method = methods.Single();

            if ( !method.SelectSelfAndAncestors( m => m.OverriddenMethod )
                .SelectMany( m => m.GetAttributes() )
                .Any( a => a.AttributeClass?.Equals( expectedAttributeTypeSymbol, SymbolEqualityComparer.Default ) ?? false ) )
            {
                if ( throwIfMissing )
                {
                    throw GeneralDiagnosticDescriptors.TemplateMethodMissesAttribute.CreateException( (method, expectedAttributeTypeSymbol, adviceName) );
                }
                else
                {
                    return null;
                }
            }

            return this._compilation.Factory.GetMethod( method );
        }

        public IOverrideMethodAdvice OverrideMethod( IMethod targetMethod, string defaultTemplate, AspectLinkerOptions? aspectLinkerOptions = null )
        {
            var templateMethod = this.GetTemplateMethod( defaultTemplate, typeof(OverrideMethodTemplateAttribute), nameof(this.OverrideMethod) );

            var advice = new OverrideMethodAdvice( this._aspect, targetMethod, templateMethod, aspectLinkerOptions );
            this._advices.Add( advice );

            return advice;
        }

        public IIntroduceMethodAdvice IntroduceMethod(
            INamedType targetType,
            string defaultTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AspectLinkerOptions? aspectLinkerOptions = null )
        {
            var templateMethod = this.GetTemplateMethod( defaultTemplate, typeof(IntroduceMethodTemplateAttribute), nameof(this.IntroduceMethod) );

            var advice = new IntroduceMethodAdvice( this._aspect, targetType, templateMethod, scope, conflictBehavior, aspectLinkerOptions );
            this._advices.Add( advice );

            return advice;
        }

        public IOverrideFieldOrPropertyAdvice OverrideFieldOrProperty( IFieldOrProperty targetDeclaration, string defaultTemplate, AspectLinkerOptions? aspectLinkerOptions = null )
        {
            // Set template represents both set and init accessors.
            var getTemplateMethod = this.GetTemplateMethod( $"get_{defaultTemplate}", typeof( OverrideFieldOrPropertyTemplateAttribute ), nameof( this.OverrideFieldOrProperty ) );
            var setTemplateMethod = this.GetTemplateMethod( $"set_{defaultTemplate}", typeof( OverrideFieldOrPropertyTemplateAttribute ), nameof( this.OverrideFieldOrProperty ) );

            var advice = new OverrideFieldOrPropertyAdvice( this._aspect, targetDeclaration, getTemplateMethod, setTemplateMethod, aspectLinkerOptions );
            this._advices.Add( advice );

            return advice;
        }

        public IOverrideFieldOrPropertyAdvice OverrideFieldOrPropertyAccessors( IFieldOrProperty targetDeclaration, string? defaultGetTemplate, string? setTemplate, AspectLinkerOptions? aspectLinkerOptions = null )
        {
            // Set template represents both set and init accessors.
            var getTemplateMethod = this.GetTemplateMethod( defaultGetTemplate, typeof( OverrideFieldOrPropertyGetTemplateAttribute ), nameof( this.OverrideFieldOrPropertyAccessors ) );
            var setTemplateMethod = this.GetTemplateMethod( setTemplate, typeof( OverrideFieldOrPropertySetTemplateAttribute ), nameof( this.OverrideFieldOrPropertyAccessors ) );

            var advice = new OverrideFieldOrPropertyAdvice( this._aspect, targetDeclaration, getTemplateMethod, setTemplateMethod, aspectLinkerOptions );
            this._advices.Add( advice );

            return advice;
        }

        public IIntroduceFieldAdvice IntroduceField( INamedType targetType, IntroductionScope scope = IntroductionScope.Default, ConflictBehavior conflictBehavior = ConflictBehavior.Default, AspectLinkerOptions? aspectLinkerOptions = null )
        {
            throw new NotImplementedException();
        }

        public IIntroducePropertyAdvice IntroduceProperty( INamedType targetType, string? defaultGetTemplate, string? setTemplate, IntroductionScope scope = IntroductionScope.Default, ConflictBehavior conflictBehavior = ConflictBehavior.Default, AspectLinkerOptions? aspectLinkerOptions = null )
        {
            throw new NotImplementedException();
        }

        public IOverrideEventAdvice OverrideEventAccessors( IEvent targetDeclaration, string? addTemplate, string? removeTemplate, string? invokeTemplate, AspectLinkerOptions? aspectLinkerOptions = null )
        {
            throw new NotImplementedException();
        }

        public IIntroducePropertyAdvice IntroduceEvent( INamedType targetType, string? addTemplate, string? removeTemplate, string? invokeTemplate = null, IntroductionScope scope = IntroductionScope.Default, ConflictBehavior conflictBehavior = ConflictBehavior.Default, AspectLinkerOptions? aspectLinkerOptions = null )
        {
            throw new NotImplementedException();
        }

        public IAdviceFactory ForLayer( string layerName ) => throw new NotImplementedException();
    }
}