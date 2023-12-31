// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
    public class ActiveDirectorySiteCollection : CollectionBase
    {
        internal DirectoryEntry? de;
        internal bool initialized;
        internal DirectoryContext? context;

        internal ActiveDirectorySiteCollection() { }

        internal ActiveDirectorySiteCollection(ArrayList sites)
        {
            for (int i = 0; i < sites.Count; i++)
                Add((ActiveDirectorySite)sites[i]!);
        }

        public ActiveDirectorySite this[int index]
        {
            get => (ActiveDirectorySite)InnerList[index]!;
            set
            {
                ActiveDirectorySite site = (ActiveDirectorySite)value;

                if (site == null)
                    throw new ArgumentNullException(nameof(value));

                if (!site.existing)
                    throw new InvalidOperationException(SR.Format(SR.SiteNotCommitted, site.Name));

                if (!Contains(site))
                    List[index] = site;
                else
                    throw new ArgumentException(SR.Format(SR.AlreadyExistingInCollection, site), nameof(value));
            }
        }

        public int Add(ActiveDirectorySite site)
        {
            ArgumentNullException.ThrowIfNull(site);

            if (!site.existing)
                throw new InvalidOperationException(SR.Format(SR.SiteNotCommitted, site.Name));

            if (!Contains(site))
                return List.Add(site);
            else
                throw new ArgumentException(SR.Format(SR.AlreadyExistingInCollection, site), nameof(site));
        }

        public void AddRange(ActiveDirectorySite[] sites)
        {
            ArgumentNullException.ThrowIfNull(sites);

            for (int i = 0; ((i) < (sites.Length)); i = ((i) + (1)))
                this.Add(sites[i]);
        }

        public void AddRange(ActiveDirectorySiteCollection sites)
        {
            ArgumentNullException.ThrowIfNull(sites);

            int count = sites.Count;
            for (int i = 0; i < count; i++)
                this.Add(sites[i]);
        }

        public bool Contains(ActiveDirectorySite site)
        {
            ArgumentNullException.ThrowIfNull(site);

            if (!site.existing)
                throw new InvalidOperationException(SR.Format(SR.SiteNotCommitted, site.Name));

            string dn = (string)PropertyManager.GetPropertyValue(site.context, site.cachedEntry, PropertyManager.DistinguishedName)!;

            for (int i = 0; i < InnerList.Count; i++)
            {
                ActiveDirectorySite tmp = (ActiveDirectorySite)InnerList[i]!;
                string tmpDn = (string)PropertyManager.GetPropertyValue(tmp.context, tmp.cachedEntry, PropertyManager.DistinguishedName)!;

                if (Utils.Compare(tmpDn, dn) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        public void CopyTo(ActiveDirectorySite[] array, int index)
        {
            List.CopyTo(array, index);
        }

        public int IndexOf(ActiveDirectorySite site)
        {
            ArgumentNullException.ThrowIfNull(site);

            if (!site.existing)
                throw new InvalidOperationException(SR.Format(SR.SiteNotCommitted, site.Name));

            string dn = (string)PropertyManager.GetPropertyValue(site.context, site.cachedEntry, PropertyManager.DistinguishedName)!;

            for (int i = 0; i < InnerList.Count; i++)
            {
                ActiveDirectorySite tmp = (ActiveDirectorySite)InnerList[i]!;
                string tmpDn = (string)PropertyManager.GetPropertyValue(tmp.context, tmp.cachedEntry, PropertyManager.DistinguishedName)!;

                if (Utils.Compare(tmpDn, dn) == 0)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, ActiveDirectorySite site)
        {
            ArgumentNullException.ThrowIfNull(site);

            if (!site.existing)
                throw new InvalidOperationException(SR.Format(SR.SiteNotCommitted, site.Name));

            if (!Contains(site))
                List.Insert(index, site);
            else
                throw new ArgumentException(SR.Format(SR.AlreadyExistingInCollection, site), nameof(site));
        }

        public void Remove(ActiveDirectorySite site)
        {
            ArgumentNullException.ThrowIfNull(site);

            if (!site.existing)
                throw new InvalidOperationException(SR.Format(SR.SiteNotCommitted, site.Name));

            string dn = (string)PropertyManager.GetPropertyValue(site.context, site.cachedEntry, PropertyManager.DistinguishedName)!;

            for (int i = 0; i < InnerList.Count; i++)
            {
                ActiveDirectorySite tmp = (ActiveDirectorySite)InnerList[i]!;
                string tmpDn = (string)PropertyManager.GetPropertyValue(tmp.context, tmp.cachedEntry, PropertyManager.DistinguishedName)!;

                if (Utils.Compare(tmpDn, dn) == 0)
                {
                    List.Remove(tmp);
                    return;
                }
            }

            // something that does not exist in the collectio
            throw new ArgumentException(SR.Format(SR.NotFoundInCollection, site), nameof(site));
        }

        protected override void OnClearComplete()
        {
            // if the property exists, clear it out
            if (initialized)
            {
                try
                {
                    if (de!.Properties.Contains("siteList"))
                        de.Properties["siteList"].Clear();
                }
                catch (COMException e)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(context, e);
                }
            }
        }

#pragma warning disable CS8765 // Nullability doesn't match overridden member
        protected override void OnInsertComplete(int index, object value)
#pragma warning restore CS8765
        {
            if (initialized)
            {
                ActiveDirectorySite site = (ActiveDirectorySite)value;
                string dn = (string)PropertyManager.GetPropertyValue(site.context, site.cachedEntry, PropertyManager.DistinguishedName)!;
                try
                {
                    de!.Properties["siteList"].Add(dn);
                }
                catch (COMException e)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(context, e);
                }
            }
        }

#pragma warning disable CS8765 // Nullability doesn't match overridden member
        protected override void OnRemoveComplete(int index, object value)
#pragma warning restore CS8765
        {
            ActiveDirectorySite site = (ActiveDirectorySite)value;
            string dn = (string)PropertyManager.GetPropertyValue(site.context, site.cachedEntry, PropertyManager.DistinguishedName)!;
            try
            {
                de!.Properties["siteList"].Remove(dn);
            }
            catch (COMException e)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(context, e);
            }
        }

#pragma warning disable CS8765 // Nullability doesn't match overridden member
        protected override void OnSetComplete(int index, object oldValue, object newValue)
#pragma warning restore CS8765
        {
            ActiveDirectorySite newsite = (ActiveDirectorySite)newValue;
            string newdn = (string)PropertyManager.GetPropertyValue(newsite.context, newsite.cachedEntry, PropertyManager.DistinguishedName)!;
            try
            {
                de!.Properties["siteList"][index] = newdn;
            }
            catch (COMException e)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(context, e);
            }
        }

        protected override void OnValidate(object value)
        {
            ArgumentNullException.ThrowIfNull(value);

            if (!(value is ActiveDirectorySite))
                throw new ArgumentException(null, nameof(value));

            if (!((ActiveDirectorySite)value).existing)
                throw new InvalidOperationException(SR.Format(SR.SiteNotCommitted, ((ActiveDirectorySite)value).Name));
        }
    }
}
