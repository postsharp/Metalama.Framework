﻿using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Advices
{
    /// <summary>
    /// The base interface for all advices with a weakly-typed <see cref="TargetDeclaration"/>.
    /// See <see cref="IAdvice{T}"/> for the strongly-typed variant. Instances of
    /// <see cref="IAdvice"/> can be instantiated thanks to the <see cref="IAdviceFactory"/> interface.
    /// </summary>
    public interface IAdvice
    {
        /// <summary>
        /// Gets the aspect that contains the advice.
        /// </summary>
        IAspect Aspect { get; }

        /// <summary>
        /// Gets the element of code to which the current advice has been applied.
        /// </summary>
        ICodeElement TargetDeclaration { get; }

        /// <summary>
        /// Gets or sets the aspect part to which the advice belongs, or <c>null</c> if it belongs to the default part.
        /// Aspect parts must be declared using the <see cref="ProvidesAspectPartsAttribute"/> custom attribute.
        /// </summary>
        public string PartName { get; set; }
    }

    /// <summary>
    /// The base interface for all advices with a strongly-typed <see cref="TargetDeclaration"/>.
    /// Instances of <see cref="IAdvice{T}"/> can be instantiated thanks to the <see cref="IAdviceFactory"/> interface.
    /// </summary>
    /// <typeparam name="T">Type of code element to which the advice can be added.</typeparam>
    public interface IAdvice<out T> : IAdvice
        where T : ICodeElement
    {
        /// <summary>
        /// Gets the element of code to which the current advice has been applied.
        /// </summary>
        new T TargetDeclaration { get; }
    }
}
