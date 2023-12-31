// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Runtime.Serialization;


namespace System.Xml
{
    [Flags]
    public enum XmlDictionaryReaderQuotaTypes
    {
        MaxDepth = 0x01,
        MaxStringContentLength = 0x02,
        MaxArrayLength = 0x04,
        MaxBytesPerRead = 0x08,
        MaxNameTableCharCount = 0x10
    }

    public sealed class XmlDictionaryReaderQuotas
    {
        private bool _readOnly;
        private int _maxStringContentLength;
        private int _maxArrayLength;
        private int _maxDepth;
        private int _maxNameTableCharCount;
        private int _maxBytesPerRead;
        private XmlDictionaryReaderQuotaTypes _modifiedQuotas;
        private const int DefaultMaxDepth = 32;
        private const int DefaultMaxStringContentLength = 8192;
        private const int DefaultMaxArrayLength = 16384;
        private const int DefaultMaxBytesPerRead = 4096;
        private const int DefaultMaxNameTableCharCount = 16384;
        private static readonly XmlDictionaryReaderQuotas s_maxQuota = new XmlDictionaryReaderQuotas(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue,
            XmlDictionaryReaderQuotaTypes.MaxDepth | XmlDictionaryReaderQuotaTypes.MaxStringContentLength | XmlDictionaryReaderQuotaTypes.MaxArrayLength | XmlDictionaryReaderQuotaTypes.MaxBytesPerRead | XmlDictionaryReaderQuotaTypes.MaxNameTableCharCount);

        public XmlDictionaryReaderQuotas()
        {
            _maxDepth = DefaultMaxDepth;
            _maxStringContentLength = DefaultMaxStringContentLength;
            _maxArrayLength = DefaultMaxArrayLength;
            _maxBytesPerRead = DefaultMaxBytesPerRead;
            _maxNameTableCharCount = DefaultMaxNameTableCharCount;
        }

        private XmlDictionaryReaderQuotas(int maxDepth, int maxStringContentLength, int maxArrayLength, int maxBytesPerRead, int maxNameTableCharCount, XmlDictionaryReaderQuotaTypes modifiedQuotas)
        {
            _maxDepth = maxDepth;
            _maxStringContentLength = maxStringContentLength;
            _maxArrayLength = maxArrayLength;
            _maxBytesPerRead = maxBytesPerRead;
            _maxNameTableCharCount = maxNameTableCharCount;
            _modifiedQuotas = modifiedQuotas;
            MakeReadOnly();
        }

        public static XmlDictionaryReaderQuotas Max
        {
            get
            {
                return s_maxQuota;
            }
        }

        public void CopyTo(XmlDictionaryReaderQuotas quotas)
        {
            ArgumentNullException.ThrowIfNull(quotas);

            if (quotas._readOnly)
                throw new InvalidOperationException(SR.QuotaCopyReadOnly);

            InternalCopyTo(quotas);
        }

        internal void InternalCopyTo(XmlDictionaryReaderQuotas quotas)
        {
            quotas._maxStringContentLength = _maxStringContentLength;
            quotas._maxArrayLength = _maxArrayLength;
            quotas._maxDepth = _maxDepth;
            quotas._maxNameTableCharCount = _maxNameTableCharCount;
            quotas._maxBytesPerRead = _maxBytesPerRead;
            quotas._modifiedQuotas = _modifiedQuotas;
        }

        [DefaultValue(DefaultMaxStringContentLength)]
        public int MaxStringContentLength
        {
            get
            {
                return _maxStringContentLength;
            }
            set
            {
                if (_readOnly)
                    throw new InvalidOperationException(SR.Format(SR.QuotaIsReadOnly, "MaxStringContentLength"));
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
                _maxStringContentLength = value;
                _modifiedQuotas |= XmlDictionaryReaderQuotaTypes.MaxStringContentLength;
            }
        }

        [DefaultValue(DefaultMaxArrayLength)]
        public int MaxArrayLength
        {
            get
            {
                return _maxArrayLength;
            }
            set
            {
                if (_readOnly)
                    throw new InvalidOperationException(SR.Format(SR.QuotaIsReadOnly, "MaxArrayLength"));
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
                _maxArrayLength = value;
                _modifiedQuotas |= XmlDictionaryReaderQuotaTypes.MaxArrayLength;
            }
        }

        [DefaultValue(DefaultMaxBytesPerRead)]
        public int MaxBytesPerRead
        {
            get
            {
                return _maxBytesPerRead;
            }
            set
            {
                if (_readOnly)
                    throw new InvalidOperationException(SR.Format(SR.QuotaIsReadOnly, "MaxBytesPerRead"));
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

                _maxBytesPerRead = value;
                _modifiedQuotas |= XmlDictionaryReaderQuotaTypes.MaxBytesPerRead;
            }
        }

        [DefaultValue(DefaultMaxDepth)]
        public int MaxDepth
        {
            get
            {
                return _maxDepth;
            }
            set
            {
                if (_readOnly)
                    throw new InvalidOperationException(SR.Format(SR.QuotaIsReadOnly, "MaxDepth"));
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

                _maxDepth = value;
                _modifiedQuotas |= XmlDictionaryReaderQuotaTypes.MaxDepth;
            }
        }

        [DefaultValue(DefaultMaxNameTableCharCount)]
        public int MaxNameTableCharCount
        {
            get
            {
                return _maxNameTableCharCount;
            }
            set
            {
                if (_readOnly)
                    throw new InvalidOperationException(SR.Format(SR.QuotaIsReadOnly, "MaxNameTableCharCount"));
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);

                _maxNameTableCharCount = value;
                _modifiedQuotas |= XmlDictionaryReaderQuotaTypes.MaxNameTableCharCount;
            }
        }

        public XmlDictionaryReaderQuotaTypes ModifiedQuotas
        {
            get
            {
                return _modifiedQuotas;
            }
        }

        internal void MakeReadOnly()
        {
            _readOnly = true;
        }
    }
}
