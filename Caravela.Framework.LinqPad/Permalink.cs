// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using LINQPad;

namespace Caravela.Framework.LinqPad
{
    public class Permalink
    {
        public IDeclaration Declaration { get; }

        public Permalink( IDeclaration declaration )
        {
            this.Declaration = declaration;
        }

        public object? Format()
        {
            string? serializedReference;

            try
            {
                serializedReference = this.Declaration.ToRef().Serialize();
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
                var project = this.Declaration.Compilation.Project;

                return new Hyperlinq(
                    QueryLanguage.Expression,
                    $@"projectSet.GetDeclaration(@""{project.Path}"", ""{project.TargetFramework ?? ""}"", ""{serializedReference}"")",
                    "(open)" );
            }
        }
    }
}