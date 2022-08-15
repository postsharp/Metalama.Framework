// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Transformations;
using System.Collections.Immutable;

namespace Metalama.Framework.Engine.Linking;

internal partial class LinkerIntroductionStep
{
    private class MemberLevelTransformations
    {
        public ImmutableArray<LinkerInsertedStatement> Statements { get; private set; } = ImmutableArray<LinkerInsertedStatement>.Empty;

        public ImmutableArray<IntroduceParameterTransformation> Parameters { get; private set; } = ImmutableArray<IntroduceParameterTransformation>.Empty;

        public ImmutableArray<IntroduceConstructorInitializerArgumentTransformation> Arguments { get; private set; } =
            ImmutableArray<IntroduceConstructorInitializerArgumentTransformation>.Empty;

        public void Add( LinkerInsertedStatement statement ) => this.Statements = this.Statements.Add( statement );

        public void Add( IntroduceParameterTransformation transformation ) => this.Parameters = this.Parameters.Add( transformation );

        public void Add( IntroduceConstructorInitializerArgumentTransformation argument ) => this.Arguments = this.Arguments.Add( argument );

        public bool AddDefaultInitializer { get; set; }
    }

    private class TypeLevelTransformations
    {
        public bool AddExplicitDefaultConstructor { get; set; }
    }
}