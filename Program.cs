using System;
using System.Numerics;
using System.IO;
using System.Security.Cryptography;

namespace P3
{
    class Program
    {
        static void Main(string[] args) {
            if (args.Length != 9)
                throw new ArgumentException("Program requires 9 input arguments");

            // ############
            // INPUT PARAMS
            // 1) IV (in hex)
            byte[] IV = Array.ConvertAll(args[0].Split(" "), c => Convert.ToByte(c, 16));
            // Console.WriteLine($"\nIV: {BitConverter.ToString(IV).Replace("-", " ")}");
            
            // 2) g_e
            int g_e = int.Parse(args[1]);
            // Console.WriteLine($"\ng_e: {g_e}");

            // 3) g_c
            int g_c = int.Parse(args[2]);
            // Console.WriteLine($"\ng_c: {g_c}");

            // 4) N_e
            int N_e = int.Parse(args[3]);
            // Console.WriteLine($"\nN_e: {N_e}");
            
            // 5) N_c
            int N_c = int.Parse(args[4]);
            // Console.WriteLine($"\nN_c: {N_c}");

            // 6) x
            int x = int.Parse(args[5]);
            // Console.WriteLine($"\nx: {x}");

            // 7) gy (g^y mod N)
            BigInteger gy = BigInteger.Parse(args[6]);
            // Console.WriteLine($"\ngy: {gy}");

            // 8) C (encrypted message in hex)
            byte[] C = Array.ConvertAll(args[7].Split(" "), c => Convert.ToByte(c, 16));
            // Console.WriteLine($"\nC: {BitConverter.ToString(C).Replace("-", " ")}");

            // 9) P (plaintext message as a string)
            string P = args[8];
            // Console.WriteLine($"\nP: {P}");

            // #########
            // CONSTANTS
            BigInteger g = BigInteger.Subtract(BigInteger.Pow(2, g_e), g_c);
            // Console.WriteLine($"\ng: {g}");

            BigInteger N = BigInteger.Subtract(BigInteger.Pow(2, N_e), N_c);
            // Console.WriteLine($"\nN: {N}");

            // Shared Key
            byte[] sharedSecret = BigInteger.ModPow(gy, x, N).ToByteArray();
            // Console.WriteLine($"\nsharedSecret: {BitConverter.ToString(sharedSecret).Replace("-", " ")}");

            // Encrypt the string to an array of bytes.
            byte[] encrypted = EncryptStringToBytes_Aes(P, sharedSecret, IV);

            // Decrypt the bytes to a string.
            string decrypted = DecryptStringFromBytes_Aes(C, sharedSecret, IV);

            // the decrypted text followed by the encrypted bytes
            // Console.WriteLine($"\nDecrypted text followed by the encrypted bytes:");
            Console.WriteLine($"{decrypted},{BitConverter.ToString(encrypted).Replace("-", " ")}");
        }

        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV) {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV) {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
    }
}
