// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace System.Diagnostics
{
    public sealed partial class ProcessStartInfo
    {
        public string[] Verbs
        {
            get
            {
                string extension = Path.GetExtension(FileName);
                if (string.IsNullOrEmpty(extension))
                    return Array.Empty<string>();

                using (RegistryKey? key = Registry.ClassesRoot.OpenSubKey(extension))
                {
                    if (key == null)
                        return Array.Empty<string>();

                    string? value = key.GetValue(string.Empty) as string;
                    if (string.IsNullOrEmpty(value))
                        return Array.Empty<string>();

                    using (RegistryKey? subKey = Registry.ClassesRoot.OpenSubKey(value + "\\shell"))
                    {
                        if (subKey == null)
                            return Array.Empty<string>();

                        string[] names = subKey.GetSubKeyNames();
                        ArrayBuilder<string> verbs = default;
                        foreach (string name in names)
                        {
                            if (!string.Equals(name, "new", StringComparison.OrdinalIgnoreCase))
                            {
                                verbs.Add(name);
                            }
                        }
                        return verbs.ToArray();
                    }
                }
            }
        }

        public bool UseShellExecute { get; set; }
    }
}
