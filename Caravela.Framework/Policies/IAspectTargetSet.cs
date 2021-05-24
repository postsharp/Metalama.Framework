using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;
using System.Linq.Expressions;

namespace Caravela.Framework.Policies
{
    /// <summary>
    /// Represents a set of aspect targets and offers the ability to add aspects to them.
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    public interface IAspectTargetSet<out TTarget> 
        where TTarget : class, IAspectTarget
    {
        void AddAspect<TAspect>( Expression<Func<INamedType, TAspect>> expression )
            where TAspect : Attribute, IAspect<TTarget>;
        
        void AddAspect<TAspect>()
            where TAspect : Attribute, IAspect<TTarget>, new();
    }
}