using Caravela.Framework.Code;
using Caravela.Framework.Validation;
using System;
using System.Linq;

namespace Caravela.Framework.Project
{
    [InternalImplement]
    public interface IProjectPolicyBuilder
    {
        // The builder intentionally does not give write access to project properties. All configuration must use IProjectExtension.

        IProject Project { get; }
        
        // The builder intentionally does not give access to any ICompilation because project policies are compilation-independent.
        // AddAspects is designed to capture the query expression and not the results of the query results. The query can be executed
        // against introduced declarations.
        INamedTypeSet WithTypes( Func<IQueryableCompilation, IQueryable<INamedType>> typeQuery );

        
        /// <summary>
        /// Adds a validator, which gets executed after all aspects have been added to the compilation.
        /// </summary>
        /// <param name="validator"></param>
        void AddValidator( Action<ValidateDeclarationContext<ICompilation>> validator );
    }
}