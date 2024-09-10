using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Chat
{
    internal class Encryption
    {
        public char[] AlphabetNew()
        {
            String abc = "ABCDEFGHIJKLMNOPQRSTUVWXYZzyxwvutsrqponmlkjihgfedcba %?!+ç&/=-*:;.,1234567890äöüÄÖÜéèà[]{}()~";
            char[] alpha = abc.ToCharArray();
            return alpha;
        }

        //Encoding
        public char[] encodeKey(String input, String key)
        {
            char[] alpha = AlphabetNew();

            //Input wird als Array Convertiert um die verschlüsselung zu berechnen
            char[] toEncode = input.ToCharArray();
            char[] secretKey = key.ToCharArray();
            char[] encrypted = null;

            //secretKey in Nummern abspeichern
            int[] keyCode = null;
            for (int i = 0; i < secretKey.Length; i++)
            {
                for (int d = 0; d < alpha.Length; d++)
                {
                    if (secretKey[i] == alpha[d])
                    {
                        Array.Resize(ref keyCode, i + 1);
                        keyCode[i] = d;
                    }
                }
            }

            //Key Verschlüsselung
            int count = 0;
            for (int i = 0; i < toEncode.Length; i++)
            {
                for (int d = 0; d < alpha.Length; d++)
                {
                    if (toEncode[i] == alpha[d])
                    {
                        int code = d;
                        for (int k = 0; k < keyCode[count]; k++)
                        {
                            code++;
                            if (code > alpha.Length - 1)
                            {
                                code = 0;
                            }
                        }
                        Array.Resize(ref encrypted, i + 1);
                        encrypted[i] = alpha[code];
                        count++;
                        if (count > keyCode.Length - 1)
                        {
                            count = 0;
                        }
                    }
                }
            }

            return encrypted;
        }

        //Decoding
        public char[] decodeKey(String input, String key)
        {
            char[] alpha = AlphabetNew();

            char[] encrypt = input.ToCharArray();
            char[] secretKey = key.ToCharArray();

            //Save Secret Key Number Array
            int[] keyCode = null;
            for (int i = 0; i < secretKey.Length; i++)
            {
                for (int d = 0; d < alpha.Length; d++)
                {
                    if (secretKey[i] == alpha[d])
                    {
                        Array.Resize(ref keyCode, i + 1);
                        keyCode[i] = d;
                    }
                }
            }

            char[] decrypted = null;

            //Key entschlüsselung
            int count = 0;
            for (int i = 0; i < encrypt.Length; i++)
            {
                for (int d = 0; d < alpha.Length; d++)
                {
                    if (encrypt[i] == alpha[d])
                    {
                        Array.Resize(ref decrypted, i + 1);
                        int code = d;

                        //key abzählen
                        for (int k = 0; k < keyCode[count]; k++)
                        {
                            code--;
                            if (code < 0)
                            {
                                code = alpha.Length - 1;
                            }
                        }
                        decrypted[i] = alpha[code];
                        count++;
                        if (count > keyCode.Length - 1)
                        {
                            count = 0;
                        }
                    }
                }
            }
            return decrypted;
        }
    }
}
