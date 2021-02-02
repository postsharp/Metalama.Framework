// Copyright (c) "ESH-Repository" source code contributors. All Rights Reserved.
// Licensed under the Microsoft Public License (MS-PL).
// See LICENSE.md in the "ESH-Repository" root for license information.
// "ESH-Repository" root address: https://github.com/Art-Stea1th/Enhanced-Syntax-Highlighting

using Caravela.Framework.DesignTime.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.DesignTime.Vsix.Classifier
{
    internal sealed class Classifier : IClassifier, IDisposable
    {
        private readonly IClassificationTypeRegistryService _registryService;
        private static readonly IList<ClassificationSpan> _emptyList = new List<ClassificationSpan>( 0 ).AsReadOnly();

        private Document? _document;
        private Task<IReadOnlyClassifiedTextSpanCollection?>? _lastGetClassificationsTask;
        private ITextSnapshot? _snapshot;
        readonly ConcurrentQueue<SnapshotSpan> _spansQueue = new ConcurrentQueue<SnapshotSpan>();
        CancellationTokenSource? _cancellationTokenSource;

        public Classifier( ITextBuffer textBuffer, IClassificationTypeRegistryService registryService )
        {
            textBuffer.Changed += this.OnTextChanged;
            this._registryService = registryService;
        }

        private void OnTextChanged( object sender, TextContentChangedEventArgs e )
        {
            if ( this._snapshot != null && this._snapshot.Version != e.AfterVersion )
            {
                this._cancellationTokenSource?.Cancel();
                while ( !this._spansQueue.IsEmpty )
                {
                    this._spansQueue.TryDequeue( out _ );
                }
            }
        }

#pragma warning disable CS0067
        public event EventHandler<ClassificationChangedEventArgs>? ClassificationChanged;
#pragma warning restore CS0067

        IList<ClassificationSpan> IClassifier.GetClassificationSpans( SnapshotSpan span )
        {
           
            var snapshot = span.Snapshot;
            var document = snapshot.GetOpenDocumentInCurrentContextWithChanges();

            if ( document == null )
            {
                return _emptyList;
            }

            if ( !(this._document?.Id == document.Id && this._snapshot == snapshot) )
            {
                this._cancellationTokenSource?.Cancel();
                this._cancellationTokenSource = new CancellationTokenSource();

                this._document = document;
                this._snapshot = snapshot;

                this._lastGetClassificationsTask = Task.Run( () => this.GetClassificationsAsync( document, this._cancellationTokenSource.Token ) );
                
                // We start a new task here. It will not be completed synchronously so we fall back to the next block.
            }

            if ( this._lastGetClassificationsTask == null || !this._lastGetClassificationsTask.IsCompleted )
            {
                // The task is not yet complete.
                // We will raise ClassificationChanged when we will be done.
                this._spansQueue.Enqueue( span );
                return _emptyList;
            }


            var classifier = this._lastGetClassificationsTask.Result;

            if ( classifier == null )
            {
                return _emptyList;
            }

            var resultList = new List<ClassificationSpan>();

            foreach ( var classifiedSpan in classifier.GetClassifiedSpans( new TextSpan( span.Start, span.Length ) ) )
            {
                var classificationType = this.GetClassificationType( classifiedSpan.Classification );

                if ( classificationType == null )
                {
                    continue;
                }

                var snapshotSpan = new SnapshotSpan( snapshot, classifiedSpan.Span.Start, classifiedSpan.Span.Length );

                resultList.Add( new ClassificationSpan( snapshotSpan, classificationType ) );


            }


            return resultList;


            
        }

        

        private async Task<IReadOnlyClassifiedTextSpanCollection?> GetClassificationsAsync( Document document, CancellationToken cancellationToken )
        {

            var entryPoint = await DesignTimeEntryPointManager.GetServiceProviderAsync( document.Project, cancellationToken );

            var classificationService = entryPoint?.GetCompilerService<IClassificationService>();
            
            if ( classificationService == null )
            {
                // Unsupported project.
                return null;
            }

            var model = await document.GetSemanticModelAsync( cancellationToken );
            var root = await document.GetSyntaxRootAsync( cancellationToken );

            if ( model == null || root == null )
            {
                return null;
            }
          

            if ( classificationService.TryGetClassifiedTextSpans( model, root, out var classifier ) )
            {
                // Notify that we now have data for these spans.
                foreach ( var span in this._spansQueue )
                {
                    this.ClassificationChanged?.Invoke( this, new ClassificationChangedEventArgs( span )  );
                }

                return classifier;

            }


            return null;

        }

        private IClassificationType? GetClassificationType( TextSpanClassification classification )
        {
            string? name = classification switch
            {
                TextSpanClassification.Dynamic => FormatDefinitions.SpecialName,
                TextSpanClassification.TemplateKeyword => FormatDefinitions.SpecialName,
                TextSpanClassification.CompileTimeVariable => FormatDefinitions.SpecialName,
                TextSpanClassification.CompileTime => FormatDefinitions.CompileTimeName,
                _ => null
            };

            if ( name == null )
            {
                return null;
            }

            return this._registryService.GetClassificationType( name );
        }

        public void Dispose()
        {
            
        }
    }
}