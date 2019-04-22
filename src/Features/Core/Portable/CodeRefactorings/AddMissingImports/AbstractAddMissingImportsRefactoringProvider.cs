// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.AddImport;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.PasteTracking;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Shared.Utilities;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.AddMissingImports
{
    internal abstract class AbstractAddMissingImportsRefactoringProvider : CodeRefactoringProvider
    {
        private readonly IPasteTrackingService _pasteTrackingService;
        protected abstract string CodeActionTitle { get; }

        public AbstractAddMissingImportsRefactoringProvider(IPasteTrackingService pasteTrackingService)
        {
            _pasteTrackingService = pasteTrackingService;
        }

        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;

            // Currently this refactoring requires the SourceTextContainer to have a pasted text span.
            var sourceText = await document.GetTextAsync(context.CancellationToken).ConfigureAwait(false);
            if (!_pasteTrackingService.TryGetPastedTextSpan(sourceText.Container, out var textSpan))
            {
                return;
            }

            // Check pasted text span for missing imports that we can add.
            var addMissingImportsService = document.GetLanguageService<IAddMissingImportsFeatureService>();
            var usableFixes = await addMissingImportsService.GetUnambiguousFixesAsync(document, textSpan, context.CancellationToken).ConfigureAwait(false);

            if (usableFixes.IsDefaultOrEmpty)
            {
                return;
            }

            var addImportsCodeAction = new AddMissingImportsCodeAction(
                CodeActionTitle,
                cancellationToken => AddMissingImports(document, usableFixes, cancellationToken));
            context.RegisterRefactoring(addImportsCodeAction);
        }

        private async Task<Solution> AddMissingImports(Document document, ImmutableArray<AddImportFixData> fixes, CancellationToken cancellationToken)
        {
            // Add missing imports for the pasted text span.
            var addMissingImportsService = document.GetLanguageService<IAddMissingImportsFeatureService>();
            var newProject = await addMissingImportsService.AddMissingImportsAsync(document, fixes, cancellationToken).ConfigureAwait(false);
            return newProject.Solution;
        }

        private class AddMissingImportsCodeAction : CodeActions.CodeAction.SimpleCodeAction
        {
            private readonly Func<CancellationToken, Task<Solution>> _createChangedSolution;

            public AddMissingImportsCodeAction(string title, Func<CancellationToken, Task<Solution>> createChangedSolution)
                : base(title, null)
            {
                _createChangedSolution = createChangedSolution;
            }

            protected override Task<IEnumerable<CodeActionOperation>> ComputeOperationsAsync(CancellationToken cancellationToken)
            {
                return Task.FromResult<IEnumerable<CodeActionOperation>>(new[] { new LazyApplyChangesOperation(_createChangedSolution) });
            }
        }

        private sealed class LazyApplyChangesOperation : CodeActionOperation
        {
            private readonly AsyncLazy<Solution> _getChangedSolution;

            internal override bool ApplyDuringTests => true;

            public LazyApplyChangesOperation(Func<CancellationToken, Task<Solution>> createChangedSolution)
            {
                _getChangedSolution = new AsyncLazy<Solution>((cancellationToken) => createChangedSolution(cancellationToken), cacheResult: true);
            }

            public override void Apply(Workspace workspace, CancellationToken cancellationToken)
            {
                this.TryApply(workspace, new ProgressTracker(), cancellationToken);
            }

            internal override bool TryApply(Workspace workspace, IProgressTracker progressTracker, CancellationToken cancellationToken)
            {
                var changedSolution = _getChangedSolution.GetValue(cancellationToken);
                workspace.TryApplyChanges(changedSolution, progressTracker);
                return true;
            }
        }
    }
}
