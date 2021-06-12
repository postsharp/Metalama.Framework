// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Aspects;

namespace Caravela.Framework.Code
{
    /// <summary>
    /// Kinds of <see cref="IMethodBase"/>.
    /// </summary>
    [CompileTimeOnly]
    public enum MethodKind
    {
        /// <summary>
        /// Default.
        /// </summary>
        Default,

        /// <summary>
        /// Instance constructor.
        /// </summary>
        Constructor,
        
        /// <summary>
        /// Static constructor.
        /// </summary>
        StaticConstructor,
        
        /// <summary>
        /// Finalizer (destructor).
        /// </summary>
        Finalizer,

        /// <summary>
        /// Property getter.
        /// </summary>
        PropertyGet,
        
        /// <summary>
        /// Property setter.
        /// </summary>
        PropertySet,

        /// <summary>
        /// Event adder.
        /// </summary>
        EventAdd,
        
        /// <summary>
        /// Event remover.
        /// </summary>
        EventRemove,
        
        /// <summary>
        /// Event raiser.
        /// </summary>
        EventRaise,

        // DelegateInvoke
        // FunctionPointerSignature

        /// <summary>
        /// Explicit interface implementation.
        /// </summary>
        ExplicitInterfaceImplementation,

        /// <summary>
        /// Conversion operator.
        /// </summary>
        ConversionOperator,
        
        /// <summary>
        /// Other operator.
        /// </summary>
        UserDefinedOperator,

        /// <summary>
        /// Local function.
        /// </summary>
        LocalFunction
    }
}