// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host;

namespace Microsoft.CodeAnalysis.Editor.Implementation.NavigateTo
{
    internal interface INavigateToSearchService : ILanguageService
    {
        // TODO(cyrusn): Should this be an IAsyncEnumerable instead?
        Task<IEnumerable<INavigateToSearchResult>> SearchProjectAsync(Project project, string searchPattern, CancellationToken cancellationToken);
    }
}
