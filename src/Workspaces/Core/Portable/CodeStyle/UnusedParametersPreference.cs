﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.CodeStyle
{
    /// <summary>
    /// Preferences for flagging unused parameters.
    /// </summary>
    internal enum UnusedParametersPreference
    {
        // No preference, unused parameters are not flagged.
        None = 0,

        // Ununsed parameters of private methods are flagged.
        PrivateMethods = 1,

        // Unused parameters of methods with any accessibility (private/public/protected/internal) are flagged.
        AllMethods = 2,
    }
}
