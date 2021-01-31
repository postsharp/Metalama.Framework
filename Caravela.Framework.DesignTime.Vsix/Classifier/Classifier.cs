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
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.DesignTime.Vsix.Classifier
{
    internal sealed partial class Classifier : IClassifier, IDisposable
    {
        private readonly IClassificationTypeRegistryService _registryService;
        private static readonly IList<ClassificationSpan> emptyList = new List<ClassificationSpan>( 0 ).AsReadOnly();

        private Document document;
        private Task<ITextSpanClassifier> lastGetClassifierTask;
        private ITextSnapshot snapshot;
        ConcurrentQueue<SnapshotSpan> spansQueue = new ConcurrentQueue<SnapshotSpan>();
        CancellationTokenSource cancellationTokenSource;

        public Classifier( ITextBuffer textBuffer, IClassificationTypeRegistryService registryService )
        {
            textBuffer.Changed += this.OnTextChanged;
            this._registryService = registryService;
        }

        private void OnTextChanged( object sender, TextContentChangedEventArgs e )
        {
            if ( this.snapshot != null && this.snapshot.Version != e.AfterVersion )
            {
                this.cancellationTokenSource?.Cancel();
                while ( !this.spansQueue.IsEmpty )
                {
                    this.spansQueue.TryDequeue( out _ );
                }
            }
        }

#pragma warning disable CS0067
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
#pragma warning restore CS0067

        IList<ClassificationSpan> IClassifier.GetClassificationSpans( SnapshotSpan span )
        {
           
            var snapshot = span.Snapshot;
            var document = snapshot.GetOpenDocumentInCurrentContextWithChanges();

            if ( document == null )
            {
                return emptyList;
            }

            if ( !(this.document?.Id == document.Id && this.snapshot == snapshot) )
            {
                this.cancellationTokenSource?.Cancel();
                this.cancellationTokenSource = new CancellationTokenSource();

                this.document = document;
                this.snapshot = snapshot;

                this.lastGetClassifierTask = Task.Run( () => this.GetSpansAsync( document, this.cancellationTokenSource.Token ) );
                
                // We start a new task here. It will not be completed synchronously so we fall back to the next block.
            }

            if ( !this.lastGetClassifierTask.IsCompleted )
            {
                // The task is not yet complete.
                // We will raise ClassificationChanged when we will be done.
                this.spansQueue.Enqueue( span );
                return emptyList;
            }


            var classifier = this.lastGetClassifierTask.Result;

            if ( classifier == null )
            {
                return emptyList;
            }

            List<ClassificationSpan> resultList = new List<ClassificationSpan>();

            foreach ( var classifiedSpan in classifier.GetClassifiedSpans( new TextSpan( span.Start, span.Length ) ) )
            {
                var classificationType = this.GetClassificationType( classifiedSpan.category );

                if ( classificationType != null )
                {
                    var snapshotSpan = new SnapshotSpan( snapshot, classifiedSpan.span.Start, classifiedSpan.span.Length );

                    resultList.Add( new ClassificationSpan( snapshotSpan, classificationType ) );
                }
            }


            return resultList;


            
        }

        private async Task<ITextSpanClassifier> GetSpansAsync( Document document, CancellationToken cancellationToken )
        {

            var entryPoint = await DesignTimeEntryPoints.GetDesignTimeEntryPoint( document.Project, cancellationToken );

            if ( entryPoint == null )
            {
                // Unsupported project.
                return null;
            }

            var model = await document.GetSemanticModelAsync( cancellationToken );
            var root = await document.GetSyntaxRootAsync( cancellationToken );

            var projectEntryPoint = entryPoint.GetService<IProjectDesignTimeEntryPoint>();

            if ( projectEntryPoint.TryProvideClassifiedSpans( model, root, out var classifier ) )
            {
                // Notify that we now have data for these spans.
                if ( this.ClassificationChanged != null )
                {
                    foreach ( var span in this.spansQueue )
                    {
                        this.ClassificationChanged?.Invoke( this, new ClassificationChangedEventArgs( span )  );
                    }
                }

                return classifier;

            }


            return null;

        }

        private IClassificationType GetClassificationType( TextSpanCategory category )
        {
            string name = category switch
            {
                TextSpanCategory.Dynamic => FormatDefinitions.SpecialName,
                TextSpanCategory.TemplateKeyword => FormatDefinitions.SpecialName,
                TextSpanCategory.TemplateVariable => FormatDefinitions.SpecialName,
                TextSpanCategory.CompileTime => FormatDefinitions.CompileTimeName,
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
            throw new NotImplementedException();
        }
    }
}