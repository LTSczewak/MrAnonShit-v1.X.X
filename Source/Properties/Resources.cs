// Decompiled with JetBrains decompiler
// Type: ScrubCrypt.Properties.Resources
// Assembly: ScrubCrypt, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 7E6936DF-C739-4E1A-BCEF-F4BF312727BA
// Assembly location: C:\Users\hp\Downloads\Telegram Desktop\ScrubCrypt.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace MrAnonCrypter.Properties
{
    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [DebuggerNonUserCode]
    [CompilerGenerated]
    internal static class Resources
    {
        private static readonly ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        // Unique identifier for the application (replace with a randomly generated value)
        private static readonly string applicationId = "YourUniqueApplicationId";

        // SecureString to store the passphrase
        private static readonly SecureString passphrase = GetPassphrase();

        // Thread to periodically check for debugging and reverse engineering tools
        private static readonly Thread antidebugThread = new Thread(CheckAntiDebug);

        // Custom obfuscation method
        private static string ObfuscateString(string input)
        {
            // Implement a custom obfuscation method
            // Example: Reverse the string
            char[] charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        // Retrieve the passphrase from a secure source (e.g., server or hardware security module)
        private static SecureString GetPassphrase()
        {
            // Implement a secure mechanism to retrieve the passphrase dynamically
            // Example: Retrieve passphrase from a secure server
            return new SecureString();
        }

        static Resources()
        {
            // Initialize culture to the default value if not set
            resourceCulture = CultureInfo.InvariantCulture;
            resourceMan = new ResourceManager(typeof(Resources));

            // Start the anti-debugging thread
            antidebugThread.Start();
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static ResourceManager ResourceManager => resourceMan;

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get => resourceCulture;
            set => resourceCulture = value ?? CultureInfo.InvariantCulture;
        }

        internal static byte[] GetEncryptedResource(string resourceName)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var aesAlg = Aes.Create())
                {
                    // Derive key from passphrase using PBKDF2
                    using (var deriveBytes = new Rfc2898DeriveBytes(
                        ConvertToUnsecureString(passphrase),
                        Encoding.UTF8.GetBytes("SaltForDerivation"), 10000))
                    {
                        aesAlg.Key = deriveBytes.GetBytes(aesAlg.KeySize / 8);
                    }

                    aesAlg.IV = new byte[16]; // Initialization Vector (IV), ensure to use a secure method to generate this

                    using (var cryptoStream = new CryptoStream(memoryStream, aesAlg.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        // Load resource dynamically at runtime
                        var assembly = Assembly.GetExecutingAssembly();
                        var resourceStream = assembly.GetManifestResourceStream(resourceName);

                        if (resourceStream != null)
                        {
                            resourceStream.CopyTo(cryptoStream);
                            resourceStream.Close();
                        }
                    }
                }

                return memoryStream.ToArray();
            }
        }

        // Anti-debugging technique
        private static void CheckAntiDebug()
        {
            while (true)
            {
                // Implement more sophisticated anti-debugging checks here
                if (Debugger.IsAttached || IsDebuggingToolsPresent())
                {
                    Environment.FailFast("Debugger detected. Exiting.");
                }

                // Sleep for a while before the next check
                Thread.Sleep(5000); // Adjust the sleep duration based on your application's requirements
            }
        }

        // Check for the presence of debugging and reverse engineering tools
        private static bool IsDebuggingToolsPresent()
        {
            // Implement more checks for various debugging tools, virtual machines, etc.
            return false;
        }

        // Convert a SecureString to an unsecure string
        private static string ConvertToUnsecureString(SecureString secureString)
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.SecureStringToGlobalAllocUnicode(secureString);
                return Marshal.PtrToStringUni(ptr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(ptr);
            }
        }
    }
}
