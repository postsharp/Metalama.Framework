using System;
using System.Collections.Generic;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;

#pragma warning disable CS0169

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Trivia_DocComment;

public sealed class RequiredAttribute : PropertyAspect
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public string ErrorMessage { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public override void BuildAspect(IAspectBuilder<IProperty> builder)
    {
        IReadOnlyCollection<IAttribute> requiredAttribute = builder.Target.Attributes.OfAttributeType(typeof(RequiredAttribute)).ToList();

        foreach (IAttribute attribute in requiredAttribute)
        {
            builder.Advice.IntroduceAttribute(
                builder.Target,
                AttributeConstruction.Create(
                    attribute.Type.Constructors.Single(), attribute.ConstructorArguments, attribute.NamedArguments.Append(new KeyValuePair<string, TypedConstant>(nameof(RequiredAttribute.ErrorMessage), TypedConstant.Create("REQUIRED"))).ToList()),
                OverrideStrategy.Override);
        }
    }
}

public class Range : Attribute
{
    public Range(int start, int end)
    {
    }
}

// <target>
class C
{
    /// <inheritdoc/>
    [Required]
    [Range(1, 1000)]
    public int? PageSize { get; init; }
}