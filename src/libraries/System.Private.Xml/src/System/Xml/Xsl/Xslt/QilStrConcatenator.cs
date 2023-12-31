// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl.Qil;
using System.Xml.Xsl.XPath;

namespace System.Xml.Xsl.Xslt
{
    internal sealed class QilStrConcatenator
    {
        private readonly XPathQilFactory _f;
        private readonly StringBuilder _builder;
        private QilList? _concat;
        private bool _inUse;

        public QilStrConcatenator(XPathQilFactory f)
        {
            _f = f;
            _builder = new StringBuilder();
        }

        public void Reset()
        {
            Debug.Assert(!_inUse);
            _inUse = true;
            _builder.Length = 0;
            _concat = null;
        }

        private void FlushBuilder()
        {
            _concat ??= _f.BaseFactory.Sequence();
            if (_builder.Length != 0)
            {
                _concat.Add(_f.String(_builder.ToString()));
                _builder.Length = 0;
            }
        }

        public void Append(string value)
        {
            Debug.Assert(_inUse, "Reset() wasn't called");
            _builder.Append(value);
        }

        public void Append(char value)
        {
            Debug.Assert(_inUse, "Reset() wasn't called");
            _builder.Append(value);
        }

        public void Append(QilNode? value)
        {
            Debug.Assert(_inUse, "Reset() wasn't called");
            if (value != null)
            {
                Debug.Assert(value.XmlType!.TypeCode == XmlTypeCode.String);
                if (value.NodeType == QilNodeType.LiteralString)
                {
                    _builder.Append((string)(QilLiteral)value);
                }
                else
                {
                    FlushBuilder();
                    _concat!.Add(value);
                }
            }
        }

        public QilNode ToQil()
        {
            Debug.Assert(_inUse); // If we want allow multiple calls to ToQil() this logic should be changed
            _inUse = false;
            if (_concat == null)
            {
                return _f.String(_builder.ToString());
            }
            else
            {
                FlushBuilder();
                return _f.StrConcat(_concat);
            }
        }
    }
}
