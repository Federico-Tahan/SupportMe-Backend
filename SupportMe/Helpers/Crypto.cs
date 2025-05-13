using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace SupportMe.Helpers
{
     public class Crypto
        {
            //https://www.c-sharpcorner.com/article/encryption-and-decryption-using-a-symmetric-key-in-c-sharp/
            public static string EncryptSymmetricKey(string key, string plainText)
            {
                byte[] iv = new byte[16];
                byte[] array;

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(key);
                    aes.IV = iv;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                            {
                                streamWriter.Write(plainText);
                            }

                            array = memoryStream.ToArray();
                        }
                    }
                }

                return Convert.ToBase64String(array);
            }

            //https://www.c-sharpcorner.com/article/encryption-and-decryption-using-a-symmetric-key-in-c-sharp/
            public static string DecryptSymmetricKey(string key, string cipherText)
            {
                byte[] iv = new byte[16];

                try
                {
                    byte[] buffer = Convert.FromBase64String(cipherText);

                    using (Aes aes = Aes.Create())
                    {
                        aes.Key = Encoding.UTF8.GetBytes(key);
                        aes.IV = iv;
                        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                        using (MemoryStream memoryStream = new MemoryStream(buffer))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                                {
                                    return streamReader.ReadToEnd();
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    return null; //could not decrypt the string, may be the key is wrong
                }

            }

            public static string ToHexString(string str)
            {
                var sb = new StringBuilder();

                var bytes = Encoding.Unicode.GetBytes(str);
                foreach (var t in bytes)
                {
                    sb.Append(t.ToString("X2"));
                }

                return sb.ToString(); // returns: "48656C6C6F20776F726C64" for "Hello world"
            }

            public string EncrypAES128(string strKey, string strText)
            {
                try
                {
                    string original = strText;

                    using (Aes myAes = Aes.Create())
                    {
                        myAes.Mode = CipherMode.CBC;
                        myAes.KeySize = 128;
                        myAes.Padding = PaddingMode.PKCS7;
                        myAes.BlockSize = 128;
                        myAes.FeedbackSize = 128;
                        byte[] key = new byte[] { };
                        String result = "";

                        string str = strKey;
                        key = stringToByteArray(str);
                        myAes.Key = key;

                        // Encrypt the string to an array of bytes.
                        byte[] encrypted = encryptStringAES128(original, myAes.Key, myAes.IV);

                        byte[] resultado = new byte[encrypted.Length + myAes.IV.Length];
                        Array.Copy(myAes.IV, 0, resultado, 0, myAes.IV.Length);
                        Array.Copy(encrypted, 0, resultado, myAes.IV.Length, encrypted.Length);
                        return result = System.Convert.ToBase64String(resultado);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: {0}", e.Message);
                    return "";
                }
            }

            public string DecryptAES128(string strKey, string strText)
            {
                try
                {
                    string original = strText;

                    using (Aes myAes = Aes.Create())
                    {
                        myAes.Mode = CipherMode.CBC;
                        myAes.KeySize = 128;
                        myAes.Padding = PaddingMode.PKCS7;
                        myAes.BlockSize = 128;
                        myAes.FeedbackSize = 128;
                        byte[] key = new byte[] { };
                        byte[] xmlByte = new byte[] { };
                        String result = "";

                        string str = strKey;
                        key = stringToByteArray(str);

                        var base64EncodedBytes = System.Convert.FromBase64String(strText);
                        byte[] IVAES128 = new byte[16];
                        Array.Copy(base64EncodedBytes, 0, IVAES128, 0, 16);
                        myAes.IV = IVAES128;

                        base64EncodedBytes = System.Convert.FromBase64String(strText);
                        xmlByte = new byte[base64EncodedBytes.Length - 16];
                        Array.Copy(base64EncodedBytes, 16, xmlByte, 0, base64EncodedBytes.Length - 16);
                        myAes.Key = key;

                        // Encrypt the string to an array of bytes.
                        result = decryptStringAES128(xmlByte, myAes.Key, myAes.IV);
                        return result;
                    }

                }
                catch (Exception e)
                {
                    return "";
                }
            }

            private static byte[] encryptStringAES128(string plainText, byte[] Key, byte[] IV)
            {
                // Check arguments.
                if (plainText == null || plainText.Length <= 0)
                    throw new ArgumentNullException("plainText");
                if (Key == null || Key.Length <= 0)
                    throw new ArgumentNullException("Key");
                if (IV == null || IV.Length <= 0)
                    throw new ArgumentNullException("Key");
                byte[] encrypted;
                // Create an Aes object
                // with the specified key and IV.
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.KeySize = 128;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.BlockSize = 128;
                    aesAlg.FeedbackSize = 128;
                    aesAlg.Key = Key;
                    aesAlg.IV = IV;

                    // Create a decrytor to perform the stream transform.
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

            private static string decryptStringAES128(byte[] cipherText, byte[] Key, byte[] IV)
            {
                // Check arguments.
                if (cipherText == null || cipherText.Length <= 0)
                    throw new ArgumentNullException("cipherText");
                if (Key == null || Key.Length <= 0)
                    throw new ArgumentNullException("Key");
                if (IV == null || IV.Length <= 0)
                    throw new ArgumentNullException("Key");

                // Declare the string used to hold
                // the decrypted text.
                string plaintext = null;

                // Create an Aes object
                // with the specified key and IV.
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Key;
                    aesAlg.IV = IV;
                    aesAlg.Mode = CipherMode.CBC;
                    aesAlg.KeySize = 128;
                    aesAlg.Padding = PaddingMode.PKCS7;
                    aesAlg.BlockSize = 128;
                    aesAlg.FeedbackSize = 128;
                    aesAlg.Key = Key;
                    aesAlg.IV = IV;


                    // Create a decrytor to perform the stream transform.
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

            private static byte[] stringToByteArray(string hex)
            {
                return Enumerable.Range(0, hex.Length)
                                 .Where(x => x % 2 == 0)
                                 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                                 .ToArray();
            }

            public static string EncryptRC4(string key, string data)
            {
                Encoding unicode = Encoding.Unicode;

                var result = Convert.ToBase64String(Encrypt(unicode.GetBytes(key), unicode.GetBytes(data)));

                var resultHex = ToHexString(result);

                return result;
            }

            public static string DecryptRC4(string key, string data)
            {
                Encoding unicode = Encoding.Unicode;

                return unicode.GetString(Encrypt(unicode.GetBytes(key), Convert.FromBase64String(data)));
            }

            public static byte[] Encrypt(byte[] key, byte[] data)
            {
                return EncryptOutput(key, data).ToArray();
            }

            public static byte[] Decrypt(byte[] key, byte[] data)
            {
                return EncryptOutput(key, data).ToArray();
            }

            private static byte[] EncryptInitalize(byte[] key)
            {
                byte[] s = Enumerable.Range(0, 256)
                  .Select(i => (byte)i)
                  .ToArray();

                for (int i = 0, j = 0; i < 256; i++)
                {
                    j = (j + key[i % key.Length] + s[i]) & 255;

                    Swap(s, i, j);
                }

                return s;
            }

            private static IEnumerable<byte> EncryptOutput(byte[] key, IEnumerable<byte> data)
            {
                byte[] s = EncryptInitalize(key);

                int i = 0;
                int j = 0;

                return data.Select((b) =>
                {
                    i = (i + 1) & 255;
                    j = (j + s[i]) & 255;

                    Swap(s, i, j);

                    return (byte)(b ^ s[(s[i] + s[j]) & 255]);
                });
            }

            private static void Swap(byte[] s, int i, int j)
            {
                byte c = s[i];

                s[i] = s[j];
                s[j] = c;
            }

            public static string DecryptResponse(string response)
            {
                string aESSeed = "7456264DC90CD0FF7F8F71582AAA0462";
                //Charge result = null;
                string result = "";
                Crypto crypto = new Crypto();
                string decryptedString = crypto.DecryptAES128(aESSeed, response);

                using (var stringReader = new StringReader(decryptedString.Replace("\n", "")))
                {
                    //var doc = XDocument.Load(stringReader);

                    XDocument xmldoc = XDocument.Parse(decryptedString.Replace("\n", "")); //or XDocument.Load(path)
                    string jsonText = JsonConvert.SerializeXNode(xmldoc);
                    dynamic jsonResponse = JsonConvert.DeserializeObject<dynamic>(jsonText);


                    try
                    {
                        result = jsonResponse.CENTEROFPAYMENTS.reference;
                    }
                    catch (Exception e)
                    {
                    }
                }

                return result;
            }
     }
}

