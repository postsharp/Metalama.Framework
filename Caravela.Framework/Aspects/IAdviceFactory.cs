// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Code;
using System.Collections.Generic;

namespace Caravela.Framework.Aspects
{
    /// <summary>
    /// Exposes all factory methods to create advices.
    /// </summary>
    public interface IAdviceFactory
    {
        /// <summary>
        /// Creates an advice that overrides the implementation of a method.
        /// </summary>
        /// <param name="method">The method to override.</param>
        /// <param name="defaultTemplate">Name of the template method to by used by default.</param>
        /// <param name="aspectLinkerOptions">Aspect linker options.</param>
        /// <returns>An advice.</returns>
        IOverrideMethodAdvice OverrideMethod( IMethod method, string defaultTemplate, AspectLinkerOptions? aspectLinkerOptions = null );

        /// <summary>
        /// Creates an advice that introduces a new method or overrides the implementation of the existing one.
        /// </summary>
        /// <param name="targetType">The type into which the method is to be introduced.</param>
        /// <param name="defaultTemplate">Name of the template method to by used by default.</param>
        /// <param name="scope">Introduction scope.</param>
        /// <param name="conflictBehavior">Conflict behavior.</param>
        /// <param name="aspectLinkerOptions">Aspect linker options.</param>
        /// <returns></returns>
        IIntroduceMethodAdvice IntroduceMethod(
            INamedType targetType,
            string defaultTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AspectLinkerOptions? aspectLinkerOptions = null );

        IOverrideFieldOrPropertyAdvice OverrideFieldOrProperty(
            IFieldOrProperty targetDeclaration,
            string defaultTemplate,
            AspectLinkerOptions? aspectLinkerOptions = null );

        IOverrideFieldOrPropertyAdvice OverrideFieldOrPropertyAccessors(
            IFieldOrProperty targetDeclaration,
            string defaultGetTemplate,
            string? setTemplate,
            AspectLinkerOptions? aspectLinkerOptions = null );

        IIntroduceFieldAdvice IntroduceField(
            INamedType targetType,
            IntroductionScope scope = IntroductionScope.Default,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AspectLinkerOptions? aspectLinkerOptions = null );

        IIntroducePropertyAdvice IntroduceProperty(
            INamedType targetType,
            string defaultPropertyTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AspectLinkerOptions? aspectLinkerOptions = null );

        IIntroducePropertyAdvice IntroduceProperty(
            INamedType targetType,
            string name,
            string defaultGetTemplate,
            string? setTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AspectLinkerOptions? aspectLinkerOptions = null );

        IOverrideEventAdvice OverrideEventAccessors(
            IEvent targetDeclaration,
            string? addTemplate,
            string? removeTemplate,
            string? invokeTemplate,
            AspectLinkerOptions? aspectLinkerOptions = null );

        IIntroducePropertyAdvice IntroduceEvent(
            INamedType targetType,
            string addTemplate,
            string removeTemplate,
            string? invokeTemplate = null,
            IntroductionScope scope = IntroductionScope.Default,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AspectLinkerOptions? aspectLinkerOptions = null );

        IIntroduceInterfaceAdvice IntroduceInterface(
            INamedType targetType,
            INamedType interfaceType,
            bool explicitImplementation = true,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AspectLinkerOptions? aspectLinkerOptions = null );

        IIntroduceInterfaceAdvice IntroduceInterface(
            INamedType targetType,
            INamedType interfaceType,
            IReadOnlyDictionary<IMember, IMember> memberMap,
            bool explicitImplementation = true,
            ConflictBehavior conflictBehavior = ConflictBehavior.Default,
            AspectLinkerOptions? aspectLinkerOptions = null );

        /// <summary>
        /// Gets a factory objects that allows to add advices to other layers than the default one.
        /// </summary>
        /// <param name="layerName">Name of the layer to which advices created by the returned factory will belong.
        /// Layers must be declared by the aspect using <see cref="ProvidesAspectLayersAttribute"/>.
        /// </param>
        /// <returns></returns>
        IAdviceFactory ForLayer( string layerName );
    }
}