﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using System.Linq;

namespace Metalama.Framework.Engine.Advising;

internal sealed class TemplateMember<T>
    where T : class, IMemberOrNamedType
{
    public T Declaration { get; }

    public TemplateClassMember TemplateClassMember { get; }

    // Can be null in the default instance.
    public IAdviceAttribute? AdviceAttribute { get; }

    public TemplateKind SelectedKind { get; }

    public TemplateKind InterpretedKind { get; }

    public Accessibility Accessibility { get; }

    public Accessibility GetAccessorAccessibility { get; }

    public Accessibility SetAccessorAccessibility { get; }

    /// <summary>
    /// Gets a value indicating whether the method is an <c>async</c> one. If the template is a property, the value applies to the getter.
    /// </summary>
    public bool IsAsyncMethod { get; }

    /// <summary>
    /// Gets a value indicating whether the method is a <c>yield</c>-base iterator method. If the template is a property, the value applies to the getter.
    /// </summary>
    public bool IsIteratorMethod { get; }

    public TemplateMember(
        T implementation,
        TemplateClassMember templateClassMember,
        IAdviceAttribute adviceAttribute,
        TemplateKind selectedKind = TemplateKind.Default ) : this(
        implementation,
        templateClassMember,
        adviceAttribute,
        selectedKind,
        selectedKind ) { }

    public TemplateMember(
        T implementation,
        TemplateClassMember templateClassMember,
        IAdviceAttribute adviceAttribute,
        TemplateKind selectedKind,
        TemplateKind interpretedKind )
    {
        this.Declaration = implementation;
        this.TemplateClassMember = templateClassMember;
        this.AdviceAttribute = adviceAttribute.AssertNotNull();

        if ( implementation is IMethod { MethodKind: MethodKind.PropertySet or MethodKind.EventAdd or MethodKind.EventRemove }
             && templateClassMember.Parameters.Length != 1 )
        {
            throw new AssertionFailedException(
                $"'{implementation}' is an accessor but the template '{templateClassMember.Name}' has {templateClassMember.Parameters.Length} parameters." );
        }

        this.SelectedKind = selectedKind;
        this.InterpretedKind = interpretedKind != TemplateKind.None ? interpretedKind : selectedKind;

        // Get the template characteristics that may disappear or be changed during template compilation.
        var compiledTemplateAttribute = GetCompiledTemplateAttribute( implementation );

        // The one defined on the [Template] attribute has priority, then on [Accessibility],
        // the the accessibility of the template itself. The [Accessibility] attribute is added during compilation and the original
        // declaration is changed to 'public' so that it is not removed in reference assemblies.
        if ( adviceAttribute is ITemplateAttribute { Properties.Accessibility: { } templateAccessibility } )
        {
            this.Accessibility = templateAccessibility;
        }
        else
        {
            this.Accessibility = compiledTemplateAttribute.Accessibility;
            this.IsIteratorMethod = compiledTemplateAttribute.IsIteratorMethod;
            this.IsAsyncMethod = compiledTemplateAttribute.IsAsync;
        }

        if ( implementation is IProperty property )
        {
            if ( property.GetMethod != null )
            {
                var attributeOnGetter = GetCompiledTemplateAttribute( property.GetMethod );
                this.GetAccessorAccessibility = attributeOnGetter.Accessibility;
                this.IsIteratorMethod = attributeOnGetter.IsIteratorMethod;
                this.IsAsyncMethod = attributeOnGetter.IsAsync;
            }

            if ( property.SetMethod != null )
            {
                this.SetAccessorAccessibility = GetCompiledTemplateAttribute( property.SetMethod ).Accessibility;
            }
        }
    }

    private static CompiledTemplateAttribute GetCompiledTemplateAttribute( IMemberOrNamedType? declaration )
    {
        var attribute = new CompiledTemplateAttribute() { Accessibility = Accessibility.Private };

        if ( declaration == null )
        {
            return attribute;
        }

        // Set the attribute data with data computed from the code, if available.
        attribute.Accessibility = declaration.Accessibility;

        if ( declaration is IMethod method )
        {
            attribute.IsIteratorMethod = method.IsIteratorMethod() ?? false;
            attribute.IsAsync = method.IsAsync;
        }

        // Override with values stored in the CompiledTemplateAttribute.
        var attributeData = declaration.Attributes.OfAttributeType( typeof(CompiledTemplateAttribute) ).SingleOrDefault();

        if ( attributeData == null )
        {
            return attribute;
        }

        if ( attributeData.TryGetNamedArgument( nameof(CompiledTemplateAttribute.Accessibility), out var accessibility ) )
        {
            attribute.Accessibility = (Accessibility) accessibility.Value!;
        }

        if ( attributeData.TryGetNamedArgument( nameof(CompiledTemplateAttribute.IsAsync), out var isAsync ) )
        {
            attribute.IsAsync = (bool) isAsync.Value!;
        }

        if ( attributeData.TryGetNamedArgument( nameof(CompiledTemplateAttribute.IsIteratorMethod), out var isIterator ) )
        {
            attribute.IsIteratorMethod = (bool) isIterator.Value!;
        }

        return attribute;
    }

    public TemplateMember<IMemberOrNamedType> Cast()
        => TemplateMemberFactory.Create<IMemberOrNamedType>(
            this.Declaration,
            this.TemplateClassMember,
            this.AdviceAttribute.AssertNotNull(),
            this.SelectedKind,
            this.InterpretedKind );

    public TemplateMember<TOther> Cast<TOther>()
        where TOther : class, IMemberOrNamedType
        => TemplateMemberFactory.Create(
            (TOther) (IMemberOrNamedType) this.Declaration,
            this.TemplateClassMember,
            this.AdviceAttribute.AssertNotNull(),
            this.SelectedKind,
            this.InterpretedKind );

    public override string ToString() => $"{this.Declaration.Name}:{this.SelectedKind}";
}