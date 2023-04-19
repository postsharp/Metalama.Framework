using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.CodeModel.TypeParameterBuilderGeneric;

public class Aspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.IntroduceMethod(builder.Target, nameof(M), buildMethod: methodBuilder =>
        {
            var typeParameter = methodBuilder.AddTypeParameter("T");
            typeParameter.TypeKindConstraint = TypeKindConstraint.Struct;
            var typeParameterList = ((INamedType)TypeFactory.GetType(typeof(List<>))).WithTypeArguments(typeParameter);
            methodBuilder.AddParameter("arg", typeParameterList);
        });
    }

    [Template]
    void M() { }
}


// <target>
[Aspect]
class TargetCode
{
}