// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.Interactive
{
    //// TODO: This should be in a separate dll (InteractiveHost.dll)
    public sealed class InteractiveHostObject
    {
        public SearchPaths ReferencePaths { get; private set; }
        public SearchPaths SourcePaths { get; private set; }

        internal InteractiveHostObject()
        {
            ReferencePaths = new SearchPaths();
            SourcePaths = new SearchPaths();
        }
    }
}
