using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.Framework.Policies;

namespace Caravela.Framework.TestApp.Aspects.Annotations
{
    class CachedAttribute : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            var parameters = meta.Parameters.Where(p => !p.GetAnnotations<CachedAttribute>().Any<NotCachedAttribute>());
            var cacheKeyBuilder = new StringBuilder();
            cacheKeyBuilder.Append(meta.Method.DeclaringType.FullName);
            cacheKeyBuilder.Append(".");
            cacheKeyBuilder.Append(meta.Method.Name);
            cacheKeyBuilder.Append("(");
            foreach ( var p in parameters )
            {
                cacheKeyBuilder.Append(p.Value);
                cacheKeyBuilder.Append(",");
            }
            cacheKeyBuilder.Append(")");

            // TODO

            return meta.Proceed();
        }
        public override void BuildAspect(IAspectBuilder<IMethod> builder)
        {
            base.BuildAspect(builder);
        }
    }

    class ProjectPolicy : IProjectPolicy
    {
        public void BuildPolicy(IProjectPolicyBuilder builder)
        {
            builder
                .WithTypes(c => c.DeclaredTypes)
                .WithMembers(t => t.Methods.Where( m=>m.HasAspect<CachedAttribute>() ).SelectMany(m => m.Parameters))
                .AddAnnotation<CachedAttribute,NotCachedAttribute>(p => new NotCachedAttribute());
            builder.AddAnnotationRule<IParameter, CachedAttribute, NotCachedAttribute>(p => p.ParameterType.Is(typeof(CancellationToken)) ? new() : null);
        }
    }

    class NotCachedAttribute : Attribute, IAnnotation<IParameter, CachedAttribute>
    {
        public void BuildEligibility(IEligibilityBuilder<IParameter> builder)
        {
            throw new NotImplementedException();
        }
    }
}
