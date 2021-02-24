// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Advices;
using Caravela.Framework.Code;

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
        /// <returns>An advice.</returns>
        IOverrideMethodAdvice OverrideMethod( IMethod method, string defaultTemplate );

        /// <summary>
        /// Creates an advice that introduces a new method or overrides the implementation of the existing one.
        /// </summary>
        /// <param name="type">The type into which the method is to be introduced.</param>
        /// <param name="defaultTemplate">Name of the template method to by used by default.</param>
        /// <returns></returns>
        IIntroduceMethodAdvice IntroduceMethod( INamedType type, string defaultTemplate, IntroductionScope scope = IntroductionScope.Default );

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