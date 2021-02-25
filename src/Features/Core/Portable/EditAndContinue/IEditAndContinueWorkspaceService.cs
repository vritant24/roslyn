// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Debugger.Contracts.EditAndContinue;

namespace Microsoft.CodeAnalysis.EditAndContinue
{
    internal interface IEditAndContinueWorkspaceService : IWorkspaceService, IActiveStatementSpanProvider
    {
        ValueTask<ImmutableArray<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, DocumentActiveStatementSpanProvider activeStatementSpanProvider, CancellationToken cancellationToken);
        ValueTask<bool> HasChangesAsync(Solution solution, SolutionActiveStatementSpanProvider activeStatementSpanProvider, string? sourceFilePath, CancellationToken cancellationToken);
        ValueTask<(ManagedModuleUpdates Updates, ImmutableArray<DiagnosticData> Diagnostics)> EmitSolutionUpdateAsync(Solution solution, SolutionActiveStatementSpanProvider activeStatementSpanProvider, CancellationToken cancellationToken);

        void CommitSolutionUpdate();
        void DiscardSolutionUpdate();

        void OnSourceFileUpdated(Document document);
        Task OnSourceFileUpdatedAsync(Document document);

        void StartDebuggingSession(Solution solution);
        void StartEditSession(IManagedEditAndContinueDebuggerService debuggerService, out ImmutableArray<DocumentId> documentsToReanalyze);
        void EndEditSession(out ImmutableArray<DocumentId> documentsToReanalyze);
        void EndDebuggingSession(out ImmutableArray<DocumentId> documentsToReanalyze);

        ValueTask<bool?> IsActiveStatementInExceptionRegionAsync(Solution solution, ManagedInstructionId instructionId, CancellationToken cancellationToken);
        ValueTask<LinePositionSpan?> GetCurrentActiveStatementPositionAsync(Solution solution, SolutionActiveStatementSpanProvider activeStatementSpanProvider, ManagedInstructionId instructionId, CancellationToken cancellationToken);

        // Used by non-VS clients
        ValueTask<(ManagedModuleUpdates2 Updates, ImmutableArray<DiagnosticData> Diagnostics)> EmitSolutionUpdate2Async(Solution solution, CancellationToken cancellationToken);
        void StartEditSession(out ImmutableArray<DocumentId> documentsToReanalyze);
    }

    internal readonly struct ManagedModuleUpdates2
    {
        public readonly ManagedModuleUpdateStatus2 Status;

        public readonly ImmutableArray<ManagedModuleUpdate2> Updates;

        public ManagedModuleUpdates2(ManagedModuleUpdateStatus2 status, ImmutableArray<ManagedModuleUpdate2> updates)
        {
            Status = status;
            Updates = updates;
        }
    }

    internal enum ManagedModuleUpdateStatus2
    {
        None,
        Ready,
        Blocked
    }

    internal sealed class ManagedModuleUpdate2
    {
        public readonly Guid Module;
        public readonly ImmutableArray<byte> ILDelta;
        public readonly ImmutableArray<byte> MetadataDelta;
        public readonly ImmutableArray<byte> PdbDelta;
        public readonly ImmutableArray<int> UpdatedMethods;
        public ManagedModuleUpdate2(Guid module, ImmutableArray<byte> ilDelta, ImmutableArray<byte> metadataDelta, ImmutableArray<byte> pdbDelta, ImmutableArray<int> updatedMethods)
        {
            Module = module;
            ILDelta = ilDelta;
            MetadataDelta = metadataDelta;
            PdbDelta = pdbDelta;
            UpdatedMethods = updatedMethods;
        }
    }
}
