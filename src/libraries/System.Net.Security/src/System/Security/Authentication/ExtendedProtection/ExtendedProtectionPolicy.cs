// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace System.Security.Authentication.ExtendedProtection
{
    /// <summary>
    /// This class contains the necessary settings for specifying how Extended Protection
    /// should behave. Use one of the Build* methods to create an instance of this type.
    /// </summary>
    public class ExtendedProtectionPolicy : ISerializable
    {
        private readonly ServiceNameCollection? _customServiceNames;
        private readonly PolicyEnforcement _policyEnforcement;
        private readonly ProtectionScenario _protectionScenario;
        private readonly ChannelBinding? _customChannelBinding;

        public ExtendedProtectionPolicy(PolicyEnforcement policyEnforcement,
                                        ProtectionScenario protectionScenario,
                                        ServiceNameCollection? customServiceNames)
        {
            if (policyEnforcement == PolicyEnforcement.Never)
            {
                throw new ArgumentException(SR.security_ExtendedProtectionPolicy_UseDifferentConstructorForNever, nameof(policyEnforcement));
            }

            if (customServiceNames != null && customServiceNames.Count == 0)
            {
                throw new ArgumentException(SR.security_ExtendedProtectionPolicy_NoEmptyServiceNameCollection, nameof(customServiceNames));
            }

            _policyEnforcement = policyEnforcement;
            _protectionScenario = protectionScenario;
            _customServiceNames = customServiceNames;
        }

        public ExtendedProtectionPolicy(PolicyEnforcement policyEnforcement,
                                        ProtectionScenario protectionScenario,
                                        ICollection? customServiceNames)
            : this(policyEnforcement, protectionScenario,
                   customServiceNames == null ? (ServiceNameCollection?)null : new ServiceNameCollection(customServiceNames))
        {
        }

        public ExtendedProtectionPolicy(PolicyEnforcement policyEnforcement,
                                        ChannelBinding customChannelBinding)
        {
            if (policyEnforcement == PolicyEnforcement.Never)
            {
                throw new ArgumentException(SR.security_ExtendedProtectionPolicy_UseDifferentConstructorForNever, nameof(policyEnforcement));
            }
            ArgumentNullException.ThrowIfNull(customChannelBinding);

            _policyEnforcement = policyEnforcement;
            _protectionScenario = ProtectionScenario.TransportSelected;
            _customChannelBinding = customChannelBinding;
        }

        public ExtendedProtectionPolicy(PolicyEnforcement policyEnforcement)
        {
            // This is the only constructor which allows PolicyEnforcement.Never.
            _policyEnforcement = policyEnforcement;
            _protectionScenario = ProtectionScenario.TransportSelected;
        }

        [Obsolete(Obsoletions.LegacyFormatterImplMessage, DiagnosticId = Obsoletions.LegacyFormatterImplDiagId, UrlFormat = Obsoletions.SharedUrlFormat)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected ExtendedProtectionPolicy(SerializationInfo info, StreamingContext context)
        {
            throw new PlatformNotSupportedException();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new PlatformNotSupportedException();
        }

        public ServiceNameCollection? CustomServiceNames
        {
            get { return _customServiceNames; }
        }

        public PolicyEnforcement PolicyEnforcement
        {
            get { return _policyEnforcement; }
        }

        public ProtectionScenario ProtectionScenario
        {
            get { return _protectionScenario; }
        }

        public ChannelBinding? CustomChannelBinding
        {
            get { return _customChannelBinding; }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("ProtectionScenario=");
            sb.Append($"{_protectionScenario}");
            sb.Append("; PolicyEnforcement=");
            sb.Append($"{_policyEnforcement}");

            sb.Append("; CustomChannelBinding=");
            if (_customChannelBinding == null)
            {
                sb.Append("<null>");
            }
            else
            {
                sb.Append(_customChannelBinding.ToString());
            }

            sb.Append("; ServiceNames=");
            if (_customServiceNames == null)
            {
                sb.Append("<null>");
            }
            else
            {
                bool first = true;
                foreach (string serviceName in _customServiceNames)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append(", ");
                    }

                    sb.Append(serviceName);
                }
            }

            return sb.ToString();
        }

        public static bool OSSupportsExtendedProtection
        {
            get
            {
                // .NET Core is supported only on Win7+ where ExtendedProtection is supported.
                return OperatingSystem.IsWindows();
            }
        }
    }
}
