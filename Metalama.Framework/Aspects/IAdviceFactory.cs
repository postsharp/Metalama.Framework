// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Validation;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Aspects
{
    public enum FilterDirection
    {
        /// <summary>
        /// For all parameters except <c>out</c> parameters, filters the input value. For fields, properties and indexers,
        /// filters the value assigned to them and throws an exception if they are not writable.
        /// </summary>
        Default,

        /// <summary>
        /// Only filters the input value of the parameter or the value assigned to the field, property or indexer.
        /// </summary>
        Input,

        /// <summary>
        /// Only filters the output value of an <c>out</c> or <c>ref</c> parameter or the value, the value returned by the
        /// property or indexer getter, or the value assigned to the field but filtered when the field is retrieved.
        /// </summary>
        Output,

        /// <summary>
        /// Both <see cref="Input"/> and <see cref="Output"/> if this is possible.
        /// </summary>
        Both
    }

    /// <summary>
    /// Exposes all factory methods to create advice. Exposed on the <see cref="IAspectLayerBuilder.Advice"/> property
    /// of <see cref="IAspectBuilder{TAspectTarget}"/> or <see cref="IAspectLayerBuilder"/>.
    /// </summary>
    /// <seealso href="@advising-code"/>
    [InternalImplement]
    [CompileTime]
    public interface IAdviceFactory
    {
        /// <summary>
        /// Overrides the implementation of a method.
        /// </summary>
        /// <param name="targetMethod">The method to override.</param>
        /// <param name="template">Name of a method in the aspect class whose implementation will be used as a template.
        ///     This property must be annotated with <see cref="TemplateAttribute"/>. To select a different templates according to the kind of target method
        ///     (such as async or iterator methods), use the constructor of the <see cref="MethodTemplateSelector"/> type. To specify a single
        ///     template for all methods, pass a string.</param>
        /// <param name="args">An object (typically of anonymous type) whose properties map to parameters or type parameters of the template method.</param>
        /// <param name="tags">An optional opaque object of anonymous type  passed to the template method and exposed under the <see cref="meta.Tags"/> property
        ///     of the <see cref="meta"/> API.</param>
        /// <remarks>When an aspect overrides the same declaration in same aspect part multiple, the order of distinct pieces of advice is equal to the inverse of order of calls of this method.</remarks>
        /// <seealso href="@overriding-members"/>
        void Override( IMethod targetMethod, in MethodTemplateSelector template, object? args = null, object? tags = null );

        /// <summary>
        /// Introduces a new method or overrides the implementation of the existing one.
        /// </summary>
        /// <param name="targetType">The type into which the method must be introduced.</param>
        /// <param name="template">Name of the method of the aspect class that will be used as a template for the introduced method. This method must be
        ///     annotated with <see cref="TemplateAttribute"/>. This method can parameters and a return type. The actual parameters and return type
        ///     of the introduced method can be modified using the <see cref="IMethodBuilder"/> returned by this method.</param>
        /// <param name="scope">Determines the scope (e.g. <see cref="IntroductionScope.Instance"/> or <see cref="IntroductionScope.Static"/>) of the introduced
        ///     method. The default scope depends on the scope of the template method.
        ///     If the method is static, the introduced method is static. However, if the template method is non-static, then the introduced method
        ///     copies of the scope of the target declaration of the aspect.</param>
        /// <param name="whenExists">Determines the implementation strategy when a method of the same name and signature is already declared in the target type.
        ///     The default strategy is to fail with a compile-time error.</param>
        /// <param name="args">An object (typically of anonymous type) whose properties map to parameters or type parameters of the template methods.</param>
        /// <param name="tags">An optional opaque object of anonymous type  passed to the template method and exposed under the <see cref="meta.Tags"/> property
        ///     of the <see cref="meta"/> API.</param>
        /// <returns>An <see cref="IMethodBuilder"/> that allows to modify the name or signature, or to add custom attributes.</returns>
        /// <seealso href="@introducing-members"/>
        IMethodBuilder IntroduceMethod(
            INamedType targetType,
            string template,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? args = null,
            object? tags = null );

        /// <summary>
        /// Overrides a field or property by specifying a property template.
        /// </summary>
        /// <param name="targetDeclaration">The field or property to override.</param>
        /// <param name="template">The name of a property of the aspect class, with a getter, a setter, or both, whose implementation will be used as a template.
        ///     This property must be annotated with <see cref="TemplateAttribute"/>.</param>
        /// <param name="tags">An optional opaque object of anonymous type  passed to the template property and exposed under the <see cref="meta.Tags"/> property of the
        ///     <see cref="meta"/> API.</param>
        /// <remarks>When an aspect overrides the same declaration in same aspect part multiple, the order of distinct pieces of advice is equal to the inverse of order of calls of this method.</remarks>
        /// <seealso href="@overriding-members"/>
        void Override(
            IFieldOrPropertyOrIndexer targetDeclaration,
            string template,
            object? tags = null );

        /// <summary>
        /// Overrides a field or property by specifying a method template for the getter, the setter, or both.
        /// </summary>
        /// <param name="targetDeclaration">The field or property to override.</param>
        /// <param name="getTemplate">The name of the method of the aspect class whose implementation will be used as a template for the getter, or <c>null</c>
        ///     if the getter should not be overridden. This method must be annotated with <see cref="TemplateAttribute"/>. The signature of this method must
        ///     be <c>T Get()</c> where <c>T</c> is either <c>dynamic</c> or a type compatible with the type of the field or property.
        ///     To select a different templates for iterator getters, use the constructor of the <see cref="GetterTemplateSelector"/> type. To specify a single
        ///     template for all properties, pass a string.
        /// </param>
        /// <param name="setTemplate">The name of the method of the aspect class whose implementation will be used as a template for the getter, or <c>null</c>
        ///     if the getter should not be overridden. This method must be annotated with <see cref="TemplateAttribute"/>. The signature of this method must
        ///     be <c>void Set(T value</c>  where <c>T</c> is either <c>dynamic</c> or a type compatible with the type of the field or property.</param>
        /// <param name="args">An object (typically of anonymous type) whose properties map to parameters or type parameters of the template methods.</param>
        /// <param name="tags">An optional opaque object of anonymous type  passed to the template method and exposed under the <see cref="meta.Tags"/> property of the
        ///     <see cref="meta"/> API.</param>
        /// <remarks>When an aspect overrides the same declaration in same aspect part multiple, the order of distinct pieces of advice is equal to the inverse of order of calls of this method.</remarks>
        /// <seealso href="@overriding-members"/>
        void OverrideAccessors(
            IFieldOrPropertyOrIndexer targetDeclaration,
            in GetterTemplateSelector getTemplate = default,
            string? setTemplate = null,
            object? args = null,
            object? tags = null );

        /// <summary>
        /// Introduces a field to the target type.
        /// </summary>
        /// <param name="targetType">The type into which the property must be introduced.</param>
        /// <param name="name">Name of the introduced field.</param>
        /// <param name="scope">Determines the scope (e.g. <see cref="IntroductionScope.Instance"/> or <see cref="IntroductionScope.Static"/>) of the introduced
        ///     field. The default scope is <see cref="IntroductionScope.Instance"/>.</param>
        /// <param name="whenExists">Determines the implementation strategy when a property of the same name is already declared in the target type.
        ///     The default strategy is to fail with a compile-time error.</param>
        /// <param name="tags"></param>
        /// <returns>An <see cref="IPropertyBuilder"/> that allows to dynamically change the name or type of the introduced property.</returns>
        /// <seealso href="@introducing-members"/>
        IFieldBuilder IntroduceField(
            INamedType targetType,
            string name,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? tags = null );

        /// <summary>
        /// Introduces a property to the target type, or overrides the implementation of an existing one, by specifying a property template.
        /// </summary>
        /// <param name="targetType">The type into which the property must be introduced.</param>
        /// <param name="template">The name of the property in the aspect class that will be used as a template for the new property.
        ///     This property must be annotated with <see cref="TemplateAttribute"/>. The type of this property can be either <c>dynamic</c> or any specific
        ///     type. It is possible to dynamically change the type of the introduced property thanks to the <see cref="IPropertyBuilder"/> returned by
        ///     this method.
        /// </param>
        /// <param name="scope">Determines the scope (e.g. <see cref="IntroductionScope.Instance"/> or <see cref="IntroductionScope.Static"/>) of the introduced
        ///     property. The default scope depends on the scope of the template property. If the property is static, the introduced property is static. However, if the
        ///     template property is non-static, then the introduced property copies of the scope of the target declaration of the aspect.</param>
        /// <param name="whenExists">Determines the implementation strategy when a property of the same name is already declared in the target type.
        ///     The default strategy is to fail with a compile-time error.</param>
        /// <param name="tags">An optional opaque object of anonymous type  passed to the template property and exposed under the <see cref="meta.Tags"/> property of the
        ///     <see cref="meta"/> API.</param>
        /// <returns>An <see cref="IPropertyBuilder"/> that allows to dynamically change the name or type of the introduced property.</returns>
        /// <seealso href="@introducing-members"/>
        IPropertyBuilder IntroduceProperty(
            INamedType targetType,
            string template,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? tags = null );

        /// <summary>
        /// Introduces a property to the target type, or overrides the implementation of an existing one, by specifying individual template methods for each accessor. 
        /// </summary>
        /// <param name="targetType">The type into which the property must be introduced.</param>
        /// <param name="name">Name of the introduced property.</param>
        /// <param name="getTemplate">The name of the method of the aspect class whose type and implementation will be used as a template for the getter, or <c>null</c>
        ///     the introduced property should not have a getter. This method must be annotated with <see cref="TemplateAttribute"/>. The signature of this method must
        ///     be <c>T Get()</c> where <c>T</c> is either <c>dynamic</c> or a type compatible with the type of the field or property.</param>
        /// <param name="setTemplate">The name of the method of the aspect class whose type and implementation will be used as a template for the getter, or <c>null</c>
        ///     if the introduced property should not have a setter. This method must be annotated with <see cref="TemplateAttribute"/>. The signature of this method must
        ///     be <c>void Set(T value</c>  where <c>T</c> is either <c>dynamic</c> or a type compatible with the type of the field or property.</param>
        /// <param name="scope">Determines the scope (e.g. <see cref="IntroductionScope.Instance"/> or <see cref="IntroductionScope.Static"/>) of the introduced
        ///     property. The default scope depends on the scope of the template accessors. If the accessors are static, the introduced property is static. However, if the
        ///     template accessors are non-static, then the introduced property copies of the scope of the target declaration of the aspect.</param>
        /// <param name="whenExists">Determines the implementation strategy when a property of the same name is already declared in the target type.
        ///     The default strategy is to fail with a compile-time error.</param>
        /// <param name="args">An object (typically of anonymous type) whose properties map to parameters or type parameters of the template methods.</param>
        /// <param name="tags">An optional opaque object of anonymous type  passed to the template method and exposed under the <see cref="meta.Tags"/> property of the
        ///     <see cref="meta"/> API.</param>
        /// <returns>An <see cref="IPropertyBuilder"/> that allows to dynamically change the name or type of the introduced property.</returns>
        /// <seealso href="@introducing-members"/>
        IPropertyBuilder IntroduceProperty(
            INamedType targetType,
            string name,
            string? getTemplate,
            string? setTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? args = null,
            object? tags = null );

        /// <summary>
        /// Overrides an event by specifying a template for the adder, the remover, and/or the raiser.
        /// </summary>
        /// <param name="targetEvent">The event to be overridden.</param>
        /// <param name="addTemplate">The name of the method of the aspect class whose type and implementation will be used as a template for the adder, or <c>null</c>
        ///     the adder should not be overridden. This method must be annotated with <see cref="TemplateAttribute"/>. The signature of this method must
        ///     be <c>void Add(T value)</c> where <c>T</c> is either <c>dynamic</c> or a type compatible with the type of the event.</param>
        /// <param name="removeTemplate">The name of the method of the aspect class whose type and implementation will be used as a template for the remover, or <c>null</c>
        ///     the adder should not be overridden. This method must be annotated with <see cref="TemplateAttribute"/>. The signature of this method must
        ///     be <c>void Remove(T value)</c> where <c>T</c> is either <c>dynamic</c> or a type compatible with the type of the event.</param>
        /// <param name="raiseTemplate">Not yet implemented.</param>
        /// <param name="args">An object (typically of anonymous type) whose properties map to parameters or type parameters of the template methods.</param>
        /// <param name="tags">An optional opaque object of anonymous type  passed to the template method and exposed under the <see cref="meta.Tags"/> property of the
        ///     <see cref="meta"/> API.</param>
        /// <remarks>When an aspect overrides the same declaration in same aspect part multiple, the order of distinct pieces of advice is equal to the inverse of order of calls of this method.</remarks>
        /// <seealso href="@overriding-members"/>
        void OverrideAccessors(
            IEvent targetEvent,
            string? addTemplate,
            string? removeTemplate,
            string? raiseTemplate,
            object? args = null,
            object? tags = null );

        /// <summary>
        /// Introduces a new event to the target type, or overrides the implementation of an existing one, by specifying an event template.
        /// </summary>
        /// <param name="targetType">The type into which the event must be introduced.</param>
        /// <param name="eventTemplate">The name of the event in the aspect class that must be used as a template for the introduced event. This event
        ///     must be annotated with <see cref="TemplateAttribute"/>. The type of the template event can be any delegate type. The type of the introduced event
        ///     can be changed dynamically thanks to the <see cref="IEventBuilder"/> returned by this method. 
        /// </param>
        /// <param name="scope">Determines the scope (e.g. <see cref="IntroductionScope.Instance"/> or <see cref="IntroductionScope.Static"/>) of the introduced
        ///     event. The default scope depends on the scope of the template event. If the event is static, the introduced event is static. However, if the
        ///     template event is non-static, then the introduced event copies of the scope of the target declaration of the aspect.</param>
        /// <param name="whenExists">Determines the implementation strategy when an event of the same name is already declared in the target type.
        ///     The default strategy is to fail with a compile-time error.</param>
        /// <param name="tags">An optional opaque object of anonymous type  passed to the template event and exposed under the <see cref="meta.Tags"/> property of the
        ///     <see cref="meta"/> API.</param>
        /// <returns>An <see cref="IEventBuilder"/> that allows to change the name and the type of the event.</returns>
        /// <seealso href="@introducing-members"/>
        IEventBuilder IntroduceEvent(
            INamedType targetType,
            string eventTemplate,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? tags = null );

        /// <summary>
        /// Introduces a new event to the target type, or overrides the implementation of an existing one, by specifying individual template methods
        /// for the adder, the remover, and the raiser.
        /// </summary>
        /// <param name="targetType">The type into which the event must be introduced.</param>
        /// <param name="eventName">The name of the introduced event.</param>
        /// <param name="addTemplate">The name of the method of the aspect class whose type and implementation will be used as a template for the adder.
        ///     This method must be annotated with <see cref="TemplateAttribute"/>. The signature of this method must
        ///     be <c>void Add(T value)</c> where <c>T</c> is either <c>dynamic</c> or a type compatible with the type of the event.</param>
        /// <param name="removeTemplate">The name of the method of the aspect class whose type and implementation will be used as a template for the remover.
        ///     This method must be annotated with <see cref="TemplateAttribute"/>. The signature of this method must
        ///     be <c>void Add(T value)</c> where <c>T</c> is either <c>dynamic</c> or a type compatible with the type of the event.</param>
        /// <param name="raiseTemplate">Not implemented.</param>
        /// <param name="scope">Determines the scope (e.g. <see cref="IntroductionScope.Instance"/> or <see cref="IntroductionScope.Static"/>) of the introduced
        ///     event. The default scope depends on the scope of the template event. If the event is static, the introduced event is static. However, if the
        ///     template event is non-static, then the introduced event copies of the scope of the target declaration of the aspect.</param>
        /// <param name="whenExists">Determines the implementation strategy when an event of the same name is already declared in the target type.
        ///     The default strategy is to fail with a compile-time error.</param>
        /// <param name="args">An object (typically of anonymous type) whose properties map to parameters or type parameters of the template methods.</param>
        /// <param name="tags">An optional opaque object of anonymous type  passed to the template method and exposed under the <see cref="meta.Tags"/> property of the
        ///     <see cref="meta"/> API.</param>
        /// <returns>An <see cref="IEventBuilder"/> that allows to change the name and the type of the event.</returns>
        /// <seealso href="@introducing-members"/>
        IEventBuilder IntroduceEvent(
            INamedType targetType,
            string eventName,
            string addTemplate,
            string removeTemplate,
            string? raiseTemplate = null,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? args = null,
            object? tags = null );

        /// <summary>
        /// Makes a type implement a new interface specified as an <see cref="INamedType"/>.
        /// </summary>
        /// <param name="targetType">The type that must implement the new interface.</param>
        /// <param name="interfaceType">The type of the implemented interface.</param>
        /// <param name="whenExists">Determines the implementation strategy when the interface is already implemented by the target type.
        ///     The default strategy is to fail with a compile-time error.</param>
        /// <param name="tags">An optional opaque object of anonymous type  passed to templates and exposed under the <see cref="meta.Tags"/> property of the
        ///     <see cref="meta"/> API.</param>
        /// <seealso href="@implementing-interfaces"/>
        void ImplementInterface(
            INamedType targetType,
            INamedType interfaceType,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? tags = null );

        /// <summary>
        /// Makes a type implement a new interface specified as a reflection <see cref="Type"/>.
        /// </summary>
        /// <param name="targetType">The type that must implement the new interface.</param>
        /// <param name="interfaceType">The type of the implemented interface.</param>
        /// <param name="whenExists">Determines the implementation strategy when the interface is already implemented by the target type.
        ///     The default strategy is to fail with a compile-time error.</param>
        /// <param name="tags">An optional opaque object of anonymous type  passed to templates and exposed under the <see cref="meta.Tags"/> property of the
        ///     <see cref="meta"/> API.</param>
        /// <seealso href="@implementing-interfaces"/>
        void ImplementInterface(
            INamedType targetType,
            Type interfaceType,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? tags = null );

        /// <summary>
        /// Adds a type or instance initializer. 
        /// </summary>
        /// <param name="targetType">The type into which the initializer should be added.</param>
        /// <param name="template">The name of the template. This method must have no run-time parameter, be of <c>void</c> return type, and be annotated with the <see cref="TemplateAttribute"/> custom attribute.</param>
        /// <param name="kind">The type of initializer to add.</param>
        /// <param name="tags">An optional opaque object of anonymous type  passed to templates and exposed under the <see cref="meta.Tags"/> property of the
        ///     <see cref="meta"/> API.</param>
        /// <param name="args">An object (typically of anonymous type) whose properties map to parameters or type parameters of the template.</param>
        void AddInitializer(
            IMemberOrNamedType targetType,
            string template,
            InitializerKind kind,
            object? tags = null,
            object? args = null );

        void AddFilter(
            IParameter targetParameter,
            string template,
            FilterDirection direction = FilterDirection.Default,
            object? tags = null,
            object? args = null );

        void AddFilter(
            IFieldOrPropertyOrIndexer targetParameter,
            string template,
            FilterDirection direction = FilterDirection.Default,
            object? tags = null,
            object? args = null );

        [Obsolete( "Not implemented.", true )]
        void ImplementInterface(
            INamedType targetType,
            INamedType interfaceType,
            IReadOnlyList<InterfaceMemberSpecification> interfaceMemberSpecifications,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? tags = null );

        [Obsolete( "Not implemented.", true )]
        void ImplementInterface(
            INamedType targetType,
            Type interfaceType,
            IReadOnlyList<InterfaceMemberSpecification> interfaceMemberSpecifications,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? tags = null );

        [Obsolete( "Not implemented.", true )]
        void Override(
            IConstructor targetConstructor,
            string template,
            object? args = null,
            object? tags = null );

        [Obsolete( "Not implemented.", true )]
        void IntroduceConstructor(
            INamedType targetType,
            string template,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? args = null,
            object? tags = null );
    }
}