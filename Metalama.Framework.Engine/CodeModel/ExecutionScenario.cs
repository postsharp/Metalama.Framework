// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Project;

namespace Metalama.Framework.Engine.CodeModel
{
    public class ExecutionScenario : IExecutionScenario
    {
        public string Name { get; }

        public bool IsDesignTime { get; }

        public bool CapturesNonObservableTransformations { get; }

        public bool CapturesCodeFixImplementations { get; }

        public bool CapturesCodeFixTitles { get; }

        public static IExecutionScenario DesignTime { get; } = new ExecutionScenario( nameof(DesignTime), true, false, true, false );

        public static IExecutionScenario Preview { get; } = new ExecutionScenario( nameof(Preview), true, true, false, false );

        public static IExecutionScenario LiveTemplate { get; } = new ExecutionScenario( nameof(LiveTemplate), true, true, false, false );

        public static IExecutionScenario CompileTime { get; } = new ExecutionScenario( nameof(CompileTime), false, true, false, false );

        public static IExecutionScenario CodeFix { get; } = new ExecutionScenario( nameof(CodeFix), true, false, true, true );

        public static IExecutionScenario Introspection { get; } = new ExecutionScenario( nameof(Introspection), false, true, true, false );

        private ExecutionScenario(
            string name,
            bool isDesignTime,
            bool capturesNonObservableTransformations,
            bool capturesCodeFixTitles,
            bool capturesCodeFixImplementations )
        {
            this.Name = name;
            this.IsDesignTime = isDesignTime;
            this.CapturesNonObservableTransformations = capturesNonObservableTransformations;
            this.CapturesCodeFixImplementations = capturesCodeFixImplementations;
            this.CapturesCodeFixTitles = capturesCodeFixTitles;
        }
    }
}