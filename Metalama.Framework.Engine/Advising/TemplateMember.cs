// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.CompileTime;
using System.Linq;

namespace Metalama.Framework.Engine.Advising;

internal class TemplateMember<T>
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
            throw new AssertionFailedException();
        }

        this.SelectedKind = selectedKind;
        this.InterpretedKind = interpretedKind != TemplateKind.None ? interpretedKind : selectedKind;

        // Get the template accessibility. The one defined on the [Template] attribute has priority, then on [Accessibility],
        // the the accessibility of the template itself. The [Accessibility] attribute is added during compilation and the original
        // declaration is changed to 'public' so that it is not removed in reference assemblies.
        if ( adviceAttribute is ITemplateAttribute { Accessibility: { } templateAccessibility } )
        {
            this.Accessibility = templateAccessibility;
        }
        else
        {
            this.Accessibility = GetAccessibility( implementation );
        }

        if ( implementation is IProperty property )
        {
            this.GetAccessorAccessibility = GetAccessibility( property.GetMethod );
            this.SetAccessorAccessibility = GetAccessibility( property.SetMethod );
        }
    }

    private static Accessibility GetAccessibility( IMemberOrNamedType? declaration )
    {
        if ( declaration == null )
        {
            return Accessibility.Private;
        }

        var compiledTemplateAttribute = declaration.Attributes.OfAttributeType( typeof(CompiledTemplateAttribute) ).SingleOrDefault();

        if ( compiledTemplateAttribute != null && compiledTemplateAttribute.TryGetNamedArgument(
                nameof(CompiledTemplateAttribute.Accessibility),
                out var accessibility ) )
        {
            return (Accessibility) accessibility.Value!;
        }
        else
        {
            return declaration.Accessibility;
        }
    }

    public TemplateMember<IMemberOrNamedType> Cast()
        => TemplateMemberFactory.Create<IMemberOrNamedType>(
            this.Declaration,
            this.TemplateClassMember,
            this.AdviceAttribute.AssertNotNull(),
            this.SelectedKind,
            this.InterpretedKind );

    public override string ToString() => $"{this.Declaration.Name}:{this.SelectedKind}";
}