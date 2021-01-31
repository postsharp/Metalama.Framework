// Copyright (c) "ESH-Repository" source code contributors. All Rights Reserved.
// Licensed under the Microsoft Public License (MS-PL).
// See LICENSE.md in the "ESH-Repository" root for license information.
// "ESH-Repository" root address: https://github.com/Art-Stea1th/Enhanced-Syntax-Highlighting

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Classification;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.DesignTime.Vsix.Classifier
{
    internal static class Extensions
    {
        public static Task<IEnumerable<ClassifiedSpan>> GetClassifiedSpansAsync(
            this Document document, TextSpan textSpan, CancellationToken cancellationToken = default )
            => Microsoft.CodeAnalysis.Classification.Classifier.GetClassifiedSpansAsync( document, textSpan,
                cancellationToken );
    }
}