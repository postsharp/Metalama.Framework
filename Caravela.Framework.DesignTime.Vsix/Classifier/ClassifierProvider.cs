// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Caravela.Framework.DesignTime.Vsix.Classifier
{
    [Export( typeof(IClassifierProvider) )]
    [ContentType( "CSharp" )]
    [ContentType( "Basic" )]
    internal sealed class ClassifierProvider : IClassifierProvider
    {
#pragma warning disable CS0649, IDE0044, RCS1169
        [Import]
        private IClassificationTypeRegistryService? _registryService; // set via MEF
#pragma warning restore CS0649, IDE0044, RCS1169

        IClassifier? IClassifierProvider.GetClassifier( ITextBuffer textBuffer ) 
            => this._registryService != null
            ? textBuffer.Properties.GetOrCreateSingletonProperty( () => new Classifier( textBuffer, this._registryService ) )
            : null;
    }
}