// Copyright Contributors to the OpenVDB Project
// SPDX-License-Identifier: Apache-2.0
//
// PlatformConfig.cs - C# port of PlatformConfig.h
//
// This file is maintained for backwards compatibility.
// In the C++ version, PlatformConfig.h simply includes Platform.h
// In C#, this file re-exports Platform types for compatibility.

namespace OpenVDB.Platform
{
    /// <summary>
    /// Backwards compatibility - PlatformConfig is now part of Platform.
    /// This file exists for compatibility with code that imports PlatformConfig.
    /// </summary>
    /// <remarks>
    /// Note: PlatformConfig.h is marked as deprecated in the C++ version and will
    /// be removed in future versions. This C# version maintains the same deprecation status.
    /// </remarks>
    [System.Obsolete("PlatformConfig is deprecated. Use Platform directly instead.", false)]
    public static class PlatformConfigCompat
    {
        /// <summary>
        /// Version name for backwards compatibility.
        /// </summary>
        public const string VersionName = PlatformConfig.VersionName;
    }
}
