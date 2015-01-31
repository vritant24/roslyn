// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;

namespace Microsoft.CodeAnalysis.EditAndContinue
{
    [Serializable]
    internal struct ILDelta
    {
        public readonly byte[] Value;

        public ILDelta(byte[] value)
        {
            this.Value = value;
        }
    }
}
