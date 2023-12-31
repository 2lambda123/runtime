// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
    public class AttributeMetadata
    {
        private readonly string? _pszLastOriginatingDsaDN;

        private string? _originatingServerName;
        private readonly DirectoryServer _server;
        private readonly Hashtable _nameTable;
        private readonly bool _advanced;

        internal AttributeMetadata(IntPtr info, bool advanced, DirectoryServer server, Hashtable table)
        {
            if (advanced)
            {
                DS_REPL_ATTR_META_DATA_2 attrMetaData = new DS_REPL_ATTR_META_DATA_2();
                Marshal.PtrToStructure(info, attrMetaData);
                Debug.Assert(attrMetaData != null);

                Name = Marshal.PtrToStringUni(attrMetaData.pszAttributeName)!;
                Version = attrMetaData.dwVersion;
                long ftimeChangeValue = (long)((uint)attrMetaData.ftimeLastOriginatingChange1 + (((long)attrMetaData.ftimeLastOriginatingChange2) << 32));
                LastOriginatingChangeTime = DateTime.FromFileTime(ftimeChangeValue);
                LastOriginatingInvocationId = attrMetaData.uuidLastOriginatingDsaInvocationID;
                OriginatingChangeUsn = attrMetaData.usnOriginatingChange;
                LocalChangeUsn = attrMetaData.usnLocalChange;
                _pszLastOriginatingDsaDN = Marshal.PtrToStringUni(attrMetaData.pszLastOriginatingDsaDN);
            }
            else
            {
                DS_REPL_ATTR_META_DATA attrMetaData = new DS_REPL_ATTR_META_DATA();
                Marshal.PtrToStructure(info, attrMetaData);
                Debug.Assert(attrMetaData != null);

                Name = Marshal.PtrToStringUni(attrMetaData.pszAttributeName)!;
                Version = attrMetaData.dwVersion;
                long ftimeChangeValue = (long)((uint)attrMetaData.ftimeLastOriginatingChange1 + (((long)attrMetaData.ftimeLastOriginatingChange2) << 32));
                LastOriginatingChangeTime = DateTime.FromFileTime(ftimeChangeValue);
                LastOriginatingInvocationId = attrMetaData.uuidLastOriginatingDsaInvocationID;
                OriginatingChangeUsn = attrMetaData.usnOriginatingChange;
                LocalChangeUsn = attrMetaData.usnLocalChange;
            }
            _server = server;
            _nameTable = table;
            _advanced = advanced;
        }

        public string Name { get; }

        public int Version { get; }

        public DateTime LastOriginatingChangeTime { get; }

        public Guid LastOriginatingInvocationId { get; }

        public long OriginatingChangeUsn { get; }

        public long LocalChangeUsn { get; }

        public string? OriginatingServer
        {
            get
            {
                if (_originatingServerName == null)
                {
                    // check whether we have got it before
                    if (_nameTable.Contains(LastOriginatingInvocationId))
                    {
                        _originatingServerName = (string)_nameTable[LastOriginatingInvocationId]!;
                    }
                    // do the translation for downlevel platform or kcc is able to do the name translation
                    else if (!_advanced || (_advanced && _pszLastOriginatingDsaDN != null))
                    {
                        _originatingServerName = Utils.GetServerNameFromInvocationID(_pszLastOriginatingDsaDN, LastOriginatingInvocationId, _server);

                        // add it to the hashtable
                        _nameTable.Add(LastOriginatingInvocationId, _originatingServerName);
                    }
                }

                return _originatingServerName;
            }
        }
    }
}
