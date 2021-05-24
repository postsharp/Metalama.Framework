using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Validation;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Caravela.Framework.Policies
{
    [InternalImplement]
    public interface INamespacePolicyBuilder
    {
        IProject Project { get; }
        
        // The builder intentionally does not give access to the INamespace because they must be compilation-neutral.

        void AddAspect<TAspect>( Func<INamespace, IQueryable<INamedType>> typeQuery, Expression<Func<INamedType, TAspect>> createAspect )
            where TAspect : Attribute, IAspect<INamedType>;

        void AddAspect<TMember, TAspect>(
            Func<INamespace, IQueryable<INamedType>> typeQuery,
            Predicate<TMember> memberSelector,
            Expression<Func<TMember, TAspect>> createAspect )
            where TMember : class, IAspectTarget
            where TAspect : Attribute, IAspect<TMember>;
        
        void AddValidator( Action<ValidateDeclarationContext<INamespace>> validator );
        
        // TODO: Adding reference validators to namespaces is problematic
    }
}