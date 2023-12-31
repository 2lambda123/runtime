// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Xml;

namespace System.Security.Cryptography.Xml
{
    // the namespaces context corresponding to one XmlElement. the rendered list contains the namespace nodes that are actually
    // rendered to the canonicalized output. the unrendered list contains the namespace nodes that are in the node set and have
    // the XmlElement as the owner, but are not rendered.
    internal sealed class NamespaceFrame
    {
        private readonly Hashtable _rendered = new Hashtable();
        private readonly Hashtable _unrendered = new Hashtable();

        internal NamespaceFrame() { }

        internal void AddRendered(XmlAttribute attr)
        {
            _rendered.Add(Utils.GetNamespacePrefix(attr), attr);
        }

        internal XmlAttribute? GetRendered(string nsPrefix)
        {
            return (XmlAttribute?)_rendered[nsPrefix];
        }

        internal void AddUnrendered(XmlAttribute attr)
        {
            _unrendered.Add(Utils.GetNamespacePrefix(attr), attr);
        }

        internal XmlAttribute? GetUnrendered(string nsPrefix)
        {
            return (XmlAttribute?)_unrendered[nsPrefix];
        }

        internal Hashtable GetUnrendered()
        {
            return _unrendered;
        }
    }
}
