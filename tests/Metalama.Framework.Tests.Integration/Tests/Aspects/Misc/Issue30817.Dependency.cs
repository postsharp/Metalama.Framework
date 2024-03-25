﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.Issue30817
{
    public class MyAspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.IntroduceField(
                builder.Target,
                nameof(DependencyPropertyTemplate),
                buildField: f =>
                {
                    f.Name = "TheProperty";
                    f.Type = TypeFactory.GetType(typeof(object));
                    f.InitializerExpression = ExpressionFactory.Parse($"null!");
                }
            );
        }

        [Template]
        public static readonly dynamic? DependencyPropertyTemplate;
    }
  
}
