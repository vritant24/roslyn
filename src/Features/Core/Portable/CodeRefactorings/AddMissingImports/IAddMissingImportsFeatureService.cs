// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.AddImport;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.AddMissingImports
{
    internal interface IAddMissingImportsFeatureService : ILanguageService
    {
        Task<ImmutableArray<AddImportFixData>> GetUnambiguousFixesAsync(Document document, TextSpan textSpan, CancellationToken cancellationToken);

        Task<Project> AddMissingImportsAsync(Document document, ImmutableArray<AddImportFixData> fixes, CancellationToken cancellationToken);
    }
}
