// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Host;

namespace Microsoft.CodeAnalysis.CodeActions.WorkspaceServices
{
    internal interface IAddMetadataReferenceCodeActionOperationFactoryWorkspaceService : IWorkspaceService
    {
        CodeActionOperation CreateAddMetadataReferenceOperation(ProjectId projectId, AssemblyIdentity assemblyIdentity);
    }
}
