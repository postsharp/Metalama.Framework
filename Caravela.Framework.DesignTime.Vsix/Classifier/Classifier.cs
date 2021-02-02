﻿// Copyright (c) "ESH-Repository" source code contributors. All Rights Reserved.
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
    internal sealed partial class Classifier : IClassifier, IDisposable
    {
        private readonly IClassificationTypeRegistryService _registryService;
        private static readonly IList<ClassificationSpan> _emptyList = new List<ClassificationSpan>( 0 ).AsReadOnly();

        private Document _document;
        private Task<ITextSpanClassifier> _lastGetClassifierTask;
        private ITextSnapshot _snapshot;
        readonly ConcurrentQueue<SnapshotSpan> _spansQueue = new ConcurrentQueue<SnapshotSpan>();
        CancellationTokenSource _cancellationTokenSource;

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
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;
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

                this._lastGetClassifierTask = Task.Run( () => this.GetSpansAsync( document, this._cancellationTokenSource.Token ) );
                
                // We start a new task here. It will not be completed synchronously so we fall back to the next block.
            }

            if ( !this._lastGetClassifierTask.IsCompleted )
            {
                // The task is not yet complete.
                // We will raise ClassificationChanged when we will be done.
                this._spansQueue.Enqueue( span );
                return _emptyList;
            }


            var classifier = this._lastGetClassifierTask.Result;

            if ( classifier == null )
            {
                return _emptyList;
            }

            List<ClassificationSpan> resultList = new List<ClassificationSpan>();

            foreach ( var classifiedSpan in classifier.GetClassifiedSpans( new TextSpan( span.Start, span.Length ) ) )
            {
                var classificationType = this.GetClassificationType( classifiedSpan.category );

                if ( classificationType == null )
                {
                    continue;
                }

                var snapshotSpan = new SnapshotSpan( snapshot, classifiedSpan.span.Start, classifiedSpan.span.Length );

                resultList.Add( new ClassificationSpan( snapshotSpan, classificationType ) );


            }


            return resultList;


            
        }

        

        private async Task<ITextSpanClassifier> GetSpansAsync( Document document, CancellationToken cancellationToken )
        {

            var entryPoint = await DesignTimeEntryPointManager.GetDesignTimeEntryPoint( document.Project, cancellationToken );

            if ( entryPoint == null )
            {
                // Unsupported project.
                return null;
            }

            var model = await document.GetSemanticModelAsync( cancellationToken );
            var root = await document.GetSyntaxRootAsync( cancellationToken );

            var projectEntryPoint = entryPoint.GetCompilerService<IProjectDesignTimeEntryPoint>();

            if ( projectEntryPoint.TryGetTextSpanClassifier( model, root, out var classifier ) )
            {
                // Notify that we now have data for these spans.
                if ( this.ClassificationChanged != null )
                {
                    foreach ( var span in this._spansQueue )
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
                TextSpanCategory.CompileTimeVariable => FormatDefinitions.SpecialName,
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