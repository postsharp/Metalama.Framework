// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using System;

namespace Caravela.Framework.Impl.DesignTime.CodeFixes
{
    internal class CodeFixContext
    {
        public Document OriginalDocument { get; }

        public CompilationModel OriginalCompilationModel { get; }

        public IServiceProvider ServiceProvider { get; }

        public CodeFixContext(
            Document originalDocument,
            CompilationModel originalCompilationModel,
            IServiceProvider serviceProvider )
        {
            this.OriginalDocument = originalDocument;
            this.OriginalCompilationModel = originalCompilationModel;
            this.ServiceProvider = serviceProvider;
        }
    }
}