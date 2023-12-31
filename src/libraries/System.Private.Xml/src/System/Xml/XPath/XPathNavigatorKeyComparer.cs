// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using MS.Internal.Xml.Cache;

namespace System.Xml.XPath
{
    internal sealed class XPathNavigatorKeyComparer : IEqualityComparer
    {
        bool IEqualityComparer.Equals(object? obj1, object? obj2)
        {
            XPathNavigator? nav1 = obj1 as XPathNavigator;
            XPathNavigator? nav2 = obj2 as XPathNavigator;
            if ((nav1 != null) && (nav2 != null))
            {
                if (nav1.IsSamePosition(nav2))
                    return true;
            }
            return false;
        }

        int IEqualityComparer.GetHashCode(object obj)
        {
            ArgumentNullException.ThrowIfNull(obj);

            int hashCode;
            XPathNavigator? nav;
            XPathDocumentNavigator? xpdocNav;

            if (null != (xpdocNav = obj as XPathDocumentNavigator))
            {
                hashCode = xpdocNav.GetPositionHashCode();
            }
            else if (null != (nav = obj as XPathNavigator))
            {
                object? underlyingObject = nav.UnderlyingObject;
                if (underlyingObject != null)
                {
                    hashCode = underlyingObject.GetHashCode();
                }
                else
                {
                    hashCode = (int)nav.NodeType;
                    hashCode ^= nav.LocalName.GetHashCode();
                    hashCode ^= nav.Prefix.GetHashCode();
                    hashCode ^= nav.NamespaceURI.GetHashCode();
                }
            }
            else
            {
                hashCode = obj.GetHashCode();
            }
            return hashCode;
        }
    }
}
