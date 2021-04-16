// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CompileTime;

namespace Caravela.Framework.Impl.CodeModel
{
    internal class CompilationAspectSource : IAspectSource
    {
        private readonly CompilationModel _initialCompilation;
        private readonly CompileTimeAssemblyLoader _loader;

        public CompilationAspectSource( CompilationModel initialCompilation, CompileTimeAssemblyLoader loader )
        {
            this._initialCompilation = initialCompilation;
            this._loader = loader;
        }

        public AspectSourcePriority Priority => AspectSourcePriority.FromAttribute;

        public IEnumerable<INamedType> AspectTypes
        {
            get
            {
                var aspectType = this._initialCompilation.Factory.GetTypeByReflectionType( typeof( IAspect ) );
                return this._initialCompilation.GetAllAttributeTypes().Where( t => t.Is( aspectType ) && t.TypeKind == TypeKind.Class );
            }
        }

        // TODO: implement aspect exclusion based on ExcludeAspectAttribute
        public IEnumerable<ICodeElement> GetExclusions( INamedType aspectType ) => Enumerable.Empty<ICodeElement>();

        public IEnumerable<AspectInstance> GetAspectInstances( CompilationModel? compilation, AspectType aspectType ) =>
            (compilation ?? this._initialCompilation).GetAllAttributesOfType( aspectType.Type ).Select( attribute =>
            {
                var aspect = (IAspect) this._loader.CreateAttributeInstance( attribute );
                return aspectType.CreateAspectInstance( aspect, attribute.ContainingElement.AssertNotNull() );
            } );
    }
}