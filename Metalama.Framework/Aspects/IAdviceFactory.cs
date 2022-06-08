// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.DependencyInjection;
using Metalama.Framework.Validation;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Aspects
{
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
        /// Introduces a field to the target type by specifying a template.
        /// </summary>
        /// <param name="targetType">The type into which the property must be introduced.</param>
        /// <param name="template">Name of the introduced field.</param>
        /// <param name="scope">Determines the scope (e.g. <see cref="IntroductionScope.Instance"/> or <see cref="IntroductionScope.Static"/>) of the introduced
        ///     field. The default scope is <see cref="IntroductionScope.Instance"/>.</param>
        /// <param name="whenExists">Determines the implementation strategy when a property of the same name is already declared in the target type.
        ///     The default strategy is to fail with a compile-time error.</param>
        /// <param name="tags"></param>
        /// <param name="pullStrategy"></param>
        /// <returns>An <see cref="IPropertyBuilder"/> that allows to dynamically change the name or type of the introduced property.</returns>
        /// <seealso href="@introducing-members"/>
        IFieldBuilder IntroduceField(
            INamedType targetType,
            string template,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? tags = null,
            IPullStrategy? pullStrategy = null );

        /// <summary>
        /// Introduces a field to the target type by specifying a field name and <see cref="IType"/>.
        /// </summary>
        /// <param name="targetType">The type into which the property must be introduced.</param>
        /// <param name="fieldName">Name of the introduced field.</param>
        /// <param name="fieldType">Type of the introduced field.</param>
        /// <param name="scope">Determines the scope (e.g. <see cref="IntroductionScope.Instance"/> or <see cref="IntroductionScope.Static"/>) of the introduced
        ///     field. The default scope is <see cref="IntroductionScope.Instance"/>.</param>
        /// <param name="whenExists">Determines the implementation strategy when a property of the same name is already declared in the target type.
        ///     The default strategy is to fail with a compile-time error.</param>
        /// <param name="tags"></param>
        /// <param name="pullStrategy"></param>
        /// <returns>An <see cref="IPropertyBuilder"/> that allows to dynamically change the name or type of the introduced property.</returns>
        /// <seealso href="@introducing-members"/>
        IFieldBuilder IntroduceField(
            INamedType targetType,
            string fieldName,
            IType fieldType,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? tags = null,
            IPullStrategy? pullStrategy = null );

        /// <summary>
        /// Introduces a field to the target type by specifying a field name and <see cref="Type"/>.
        /// </summary>
        /// <param name="targetType">The type into which the property must be introduced.</param>
        /// <param name="fieldName">Name of the introduced field.</param>
        /// <param name="fieldType">Type of the introduced field.</param>
        /// <param name="scope">Determines the scope (e.g. <see cref="IntroductionScope.Instance"/> or <see cref="IntroductionScope.Static"/>) of the introduced
        ///     field. The default scope is <see cref="IntroductionScope.Instance"/>.</param>
        /// <param name="whenExists">Determines the implementation strategy when a property of the same name is already declared in the target type.
        ///     The default strategy is to fail with a compile-time error.</param>
        /// <param name="tags"></param>
        /// <param name="pullStrategy"></param>
        /// <returns>An <see cref="IPropertyBuilder"/> that allows to dynamically change the name or type of the introduced property.</returns>
        /// <seealso href="@introducing-members"/>
        IFieldBuilder IntroduceField(
            INamedType targetType,
            string fieldName,
            Type fieldType,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? tags = null,
            IPullStrategy? pullStrategy = null );

        /// <summary>
        /// Introduces an automatic to the target type by specifying a property name and <see cref="Type"/>.
        /// </summary>
        /// <param name="targetType">The type into which the property must be introduced.</param>
        /// <param name="propertyName">Name of the introduced field.</param>
        /// <param name="propertyType">Type of the introduced field.</param>
        /// <param name="scope">Determines the scope (e.g. <see cref="IntroductionScope.Instance"/> or <see cref="IntroductionScope.Static"/>) of the introduced
        ///     field. The default scope is <see cref="IntroductionScope.Instance"/>.</param>
        /// <param name="whenExists">Determines the implementation strategy when a property of the same name is already declared in the target type.
        ///     The default strategy is to fail with a compile-time error.</param>
        /// <param name="tags"></param>
        /// <param name="pullStrategy"></param>
        /// <returns>An <see cref="IPropertyBuilder"/> that allows to dynamically change the name or type of the introduced property.</returns>
        /// <seealso href="@introducing-members"/>
        IPropertyBuilder IntroduceAutomaticProperty(
            INamedType targetType,
            string propertyName,
            Type propertyType,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? tags = null,
            IPullStrategy? pullStrategy = null );

        /// <summary>
        /// Introduces an automatic to the target type by specifying a property name and <see cref="IType"/>.
        /// </summary>
        /// <param name="targetType">The type into which the property must be introduced.</param>
        /// <param name="propertyName">Name of the introduced field.</param>
        /// <param name="propertyType">Type of the introduced field.</param>
        /// <param name="scope">Determines the scope (e.g. <see cref="IntroductionScope.Instance"/> or <see cref="IntroductionScope.Static"/>) of the introduced
        ///     field. The default scope is <see cref="IntroductionScope.Instance"/>.</param>
        /// <param name="whenExists">Determines the implementation strategy when a property of the same name is already declared in the target type.
        ///     The default strategy is to fail with a compile-time error.</param>
        /// <param name="tags"></param>
        /// <param name="pullStrategy"></param>
        /// <returns>An <see cref="IPropertyBuilder"/> that allows to dynamically change the name or type of the introduced property.</returns>
        /// <seealso href="@introducing-members"/>
        IPropertyBuilder IntroduceAutomaticProperty(
            INamedType targetType,
            string propertyName,
            IType propertyType,
            IntroductionScope scope = IntroductionScope.Default,
            OverrideStrategy whenExists = OverrideStrategy.Default,
            object? tags = null,
            IPullStrategy? pullStrategy = null );

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
            INamedType targetType,
            string template,
            InitializerKind kind,
            object? tags = null,
            object? args = null );

        /// <summary>
        /// Adds an initializer to a specific constructor. 
        /// </summary>
        /// <param name="targetConstructor">The constructor into which the initializer should be added.</param>
        /// <param name="template">The name of the template. This method must have no run-time parameter, be of <c>void</c> return type, and be annotated with the <see cref="TemplateAttribute"/> custom attribute.</param>
        /// <param name="tags">An optional opaque object of anonymous type  passed to templates and exposed under the <see cref="meta.Tags"/> property of the
        ///     <see cref="meta"/> API.</param>
        /// <param name="args">An object (typically of anonymous type) whose properties map to parameters or type parameters of the template.</param>
        void AddInitializer( IConstructor targetConstructor, string template, object? tags = null, object? args = null );

        /// <summary>
        /// Adds a contract to a parameter. Contracts are usually used to validate parameters (pre- or post-conditions) or to normalize their value (null-to-empty, trimming, normalizing case, ...).
        /// </summary>
        /// <param name="targetParameter">The parameter to which the contract should be added.</param>
        /// <param name="template">The name of the template method. This method must have a single run-time parameter named <c>value</c>, and be annotated with the <see cref="TemplateAttribute"/> custom attribute.</param>
        /// <param name="direction">Direction of the data flow to which the contract should apply. See <see cref="ContractDirection"/> for details.</param>
        /// <param name="tags">An optional opaque object of anonymous type  passed to templates and exposed under the <see cref="meta.Tags"/> property of the
        ///     <see cref="meta"/> API.</param>
        /// <param name="args">An object (typically of anonymous type) whose properties map to parameters or type parameters of the template.</param>
        void AddContract(
            IParameter targetParameter,
            string template,
            ContractDirection direction = ContractDirection.Default,
            object? tags = null,
            object? args = null );

        /// <summary>
        /// Adds a contract to a field, property or indexer. Contracts are usually used to validate the value assigned to fields properties or indexers or to normalize their value (null-to-empty, trimming, normalizing case, ...)
        /// before assignment. Alternatively, a contract can be used to validate the value <i>returned</i> by a property or indexer, in which case the <paramref name="direction"/> parameter should be set to <see cref="ContractDirection.Output"/>.
        /// </summary>
        /// <param name="targetMember">The field, property or indexer to which the contract should be added.</param>
        /// <param name="template">The name of the template method. This method must have a single run-time parameter named <c>value</c>, and be annotated with the <see cref="TemplateAttribute"/> custom attribute.</param>
        /// <param name="direction">Direction of the data flow to which the contract should apply. See <see cref="ContractDirection"/> for details.</param>
        /// <param name="tags">An optional opaque object of anonymous type  passed to templates and exposed under the <see cref="meta.Tags"/> property of the
        ///     <see cref="meta"/> API.</param>
        /// <param name="args">An object (typically of anonymous type) whose properties map to parameters or type parameters of the template.</param>      
        void AddContract(
            IFieldOrPropertyOrIndexer targetMember,
            string template,
            ContractDirection direction = ContractDirection.Default,
            object? tags = null,
            object? args = null );

        /// <summary>
        /// Adds a custom attribute to a given declaration.
        /// </summary>
        /// <param name="targetDeclaration">The declaration to which the custom attribute should be added.</param>
        /// <param name="attribute">The custom attribute to be added. It can be an existing <see cref="IAttribute"/>, or you can use <see cref="AttributeConstruction"/>
        /// to specify a new attribute.</param>
        /// <param name="whenExists">Specifies the strategy to follow when an attribute of the same type already exists on the target declaration. <see cref="OverrideStrategy.Fail"/> will fail the
        /// compilation with an error and is the default strategy. <see cref="OverrideStrategy.Ignore"/> will silently ignore the introduction. <see cref="OverrideStrategy.Override"/> will remove
        /// all previous instances and replace them by the new one. <see cref="OverrideStrategy.New"/> will add the new instance regardless.</param>
        void AddAttribute(
            IDeclaration targetDeclaration,
            IAttributeData attribute,
            OverrideStrategy whenExists = OverrideStrategy.Default );

        /// <summary>
        /// Removes all custom attributes of a given <see cref="INamedType"/> from a given declaration.
        /// </summary>
        /// <param name="targetDeclaration">The declaration from which custom attributes have to be removed.</param>
        /// <param name="attributeType">The type of custom attributes to be removed.</param>
        void RemoveAttributes(
            IDeclaration targetDeclaration,
            INamedType attributeType );

        /// <summary>
        /// Removes all custom attributes of a given <see cref="Type"/> from a given declaration.
        /// </summary>
        /// <param name="targetDeclaration">The declaration from which custom attributes have to be removed.</param>
        /// <param name="attributeType">The type of custom attributes to be removed.</param>
        void RemoveAttributes(
            IDeclaration targetDeclaration,
            Type attributeType );

        /// <summary>
        /// Returns a copy of the current <see cref="IAdviceFactory"/> that will add advice to a specified layer of the current aspect.
        /// </summary>
        /// <param name="layerName">The name of the aspect layer. It must be defined by adding a <see cref="LayersAttribute"/> to the aspect class.</param>
        /// <returns></returns>
        IAdviceFactory ForLayer( string? layerName );

        /// <summary>
        /// Returns a copy of the current <see cref="IAdviceFactory"/> that will a specified object to find factory methods.
        /// </summary>
        /// <param name="templateProvider">Instance of an object with template members.</param>
        /// <returns>An <see cref="IAdviceFactory"/>.</returns>
        IAdviceFactory WithTemplateProvider( ITemplateProvider templateProvider );

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