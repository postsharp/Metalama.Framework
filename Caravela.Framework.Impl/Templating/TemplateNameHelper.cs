// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Impl.Templating
{
    internal static class TemplateNameHelper
    {
        public static string GetCompiledTemplateName(string templateMethodName) => templateMethodName + TemplateCompiler.TemplateMethodSuffix;

        public static string GetCompiledPropertyGetTemplateName( string propertyName ) => $"__get_{propertyName}{TemplateCompiler.TemplateMethodSuffix}";

        public static string GetCompiledPropertySetTemplateName( string propertyName ) => $"__set_{propertyName}{TemplateCompiler.TemplateMethodSuffix}";
    }
}
