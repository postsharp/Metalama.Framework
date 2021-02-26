// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;

namespace Caravela.Framework.Sdk
{
    public static class CodeModelExtensions
    {
        public static ISdkCodeElement ToSdkCodeElement( this ICodeElement codeElement ) => (ISdkCodeElement) codeElement;

        public static ISdkType ToSdkType( this IType type ) => (ISdkType) type;
    }
}