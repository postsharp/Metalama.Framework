// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using LINQPad;
using Metalama.Framework.Code;

namespace Metalama.LinqPad
{
    /// <summary>
    /// Represents a hyperlink to an <see cref="IDeclaration"/>. When clicking on a hyperlink, a new query is opened, starting at
    /// the declaration.
    /// </summary>
    internal class Permalink
    {
        private readonly GetCompilationInfo _getCompilationInfo;
        private readonly IDeclaration _declaration;

        public Permalink( GetCompilationInfo getCompilationInfo, IDeclaration declaration )
        {
            this._getCompilationInfo = getCompilationInfo;
            this._declaration = declaration;
        }

        public object? Format()
        {
            string? serializedReference;

            try
            {
                serializedReference = this._declaration.ToRef().ToSerializableId().ToString();
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
                    $@"{this._getCompilationInfo.WorkspaceExpression}.GetDeclaration(@""{project.Path}"", ""{project.TargetFramework ?? ""}"", ""{serializedReference}"", {this._getCompilationInfo.IsMetalamaOutput.ToString().ToLowerInvariant()})",
                    "(open)" );
            }
        }
    }
}