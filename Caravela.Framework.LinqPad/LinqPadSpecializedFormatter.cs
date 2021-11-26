// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Utilities.Dump;
using LINQPad;

namespace Caravela.Framework.LinqPad
{
    public class LinqPadSpecializedFormatter : LinqPadFormatter
    {
        protected override object CreateSummary<T>( object o, string summary )
        {
            DumpContainer container = new();

            Hyperlinq link = new(
                () =>
                {
                    // On click, replace the link by the object content.
                    container.Content = o;
                },
                summary );

            container.Content = link;

            return container;
        }
    }
}