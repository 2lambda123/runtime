// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace System.Resources
{
    /// <summary>
    /// Encapsulates <see cref="CultureInfo" /> fallback for resource lookup.
    /// </summary>
    internal sealed class ResourceFallbackManager : IEnumerable<CultureInfo>
    {
        private readonly CultureInfo m_startingCulture;
        private readonly CultureInfo? m_neutralResourcesCulture;
        private readonly bool m_useParents;

        internal ResourceFallbackManager(CultureInfo? startingCulture, CultureInfo? neutralResourcesCulture, bool useParents)
        {
            if (startingCulture != null)
            {
                m_startingCulture = startingCulture;
            }
            else
            {
                m_startingCulture = CultureInfo.CurrentUICulture;
            }

            m_neutralResourcesCulture = neutralResourcesCulture;
            m_useParents = useParents;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        // WARING: This function must be kept in sync with ResourceManager.GetFirstResourceSet()
        public IEnumerator<CultureInfo> GetEnumerator()
        {
            bool reachedNeutralResourcesCulture = false;

            // 1. starting culture chain, up to neutral
            CultureInfo currentCulture = m_startingCulture;
            do
            {
                if (m_neutralResourcesCulture != null && currentCulture.Name == m_neutralResourcesCulture.Name)
                {
                    // Return the invariant culture all the time, even if the UltimateResourceFallbackLocation
                    // is a satellite assembly.  This is fixed up later in ManifestBasedResourceGroveler::UltimateFallbackFixup.
                    yield return CultureInfo.InvariantCulture;
                    reachedNeutralResourcesCulture = true;
                    break;
                }
                yield return currentCulture;
                currentCulture = currentCulture.Parent;
            } while (m_useParents && !currentCulture.HasInvariantCultureName);

            if (!m_useParents || m_startingCulture.HasInvariantCultureName)
            {
                yield break;
            }

            // 2. invariant
            //    Don't return invariant twice though.
            if (reachedNeutralResourcesCulture)
                yield break;

            yield return CultureInfo.InvariantCulture;
        }
    }
}
