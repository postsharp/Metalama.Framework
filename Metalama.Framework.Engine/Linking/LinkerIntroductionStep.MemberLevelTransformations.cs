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

        public ImmutableArray<AppendParameterTransformation> Parameters { get; private set; } = ImmutableArray<AppendParameterTransformation>.Empty;

        public ImmutableArray<AppendConstructorInitializerArgumentTransformation> Arguments { get; private set; } =
            ImmutableArray<AppendConstructorInitializerArgumentTransformation>.Empty;

        public void Add( LinkerInsertedStatement statement ) => this.Statements = this.Statements.Add( statement );

        public void Add( AppendParameterTransformation transformation ) => this.Parameters = this.Parameters.Add( transformation );

        public void Add( AppendConstructorInitializerArgumentTransformation argument ) => this.Arguments = this.Arguments.Add( argument );
    }
}