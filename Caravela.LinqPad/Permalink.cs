// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using LINQPad;

namespace Caravela.LinqPad
{
    /// <summary>
    /// Represents a hyperlink to an <see cref="IDeclaration"/>. When clicking on a hyperlink, a new query is opened, starting at
    /// the declaration.
    /// </summary>
    internal class Permalink
    {
        private readonly IDeclaration _declaration;

        public Permalink( IDeclaration declaration )
        {
            this._declaration = declaration;
        }

        public object? Format()
        {
            string? serializedReference;

            try
            {
                serializedReference = this._declaration.ToRef().Serialize();
            }
            catch
            {
                // This is not implemented everywhere, so skip exceptions. 
                serializedReference = null;
            }

            if ( serializedReference == null )
            {
                return null;
            }
            else
            {
                var project = this._declaration.Compilation.Project;

                return new Hyperlinq(
                    QueryLanguage.Expression,
                    $@"projectSet.GetDeclaration(@""{project.Path}"", ""{project.TargetFramework ?? ""}"", ""{serializedReference}"")",
                    "(open)" );
            }
        }
    }
}