using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Chat
{
    internal class HellmanKeyExchange
    {
        private int P = 89;
        private int G = 3;
        public int KeyLenght = 15;

        Random rnd = new Random();

        public int[] initPrivateKey(char[] alphabet)
        {
            int[] key = new int[KeyLenght];

            for (int i = 0; i < KeyLenght; i++)
            {
                key[i] = rnd.Next(1, P);
            }
            return key;
        }

        public string generatePublicKey(int[] privateKey, char[] alphabet)
        {
            int[] pKey = new int[KeyLenght];

            for (int i = 0; i < KeyLenght; i++)
            {
                pKey[i] = ModExp(G, privateKey[i], P);
            }
            return convertToString(pKey, alphabet);
        }

        public string getSharedKey(string partnerString, int[] privateKey, char[] alphabet)
        {
            int[] partnerPublicKey = convertToInt(partnerString, alphabet);
            int[] sharedKeyNum = new int[KeyLenght];
            for (int i = 0; i < KeyLenght; i++)
            {
                sharedKeyNum[i] = ModExp(partnerPublicKey[i], privateKey[i], P);
            }
            return convertToString(sharedKeyNum, alphabet);
        }

        private string convertToString(int[] keyNum, char[] alphabet)
        {
            char[] keyChars = new char[KeyLenght];

            for (int i = 0; i < KeyLenght; i++)
            {
                keyChars[i] = alphabet[keyNum[i]];
            }

            return new string(keyChars);
        }

        private int[] convertToInt(string publicKey, char[] alphabet)
        {
            char[] letters = publicKey.ToCharArray();
            int[] key = new int[KeyLenght];
            for (int i = 0; i < letters.Length; i++)
            {
                for (int l = 0; l < alphabet.Length; l++)
                {
                    if (letters[i] == alphabet[l])
                    {
                        key[i] = l;
                    }
                }
            }

            return key;
        }

        private int ModExp(int baseValue, int exp, int mod)
        {
            long result = 1;
            long baseVal = baseValue % mod;
            while (exp > 0)
            {
                if ((exp % 2) == 1)
                {
                    result = (result * baseVal) % mod;
                }
                exp >>= 1;
                baseVal = (baseVal * baseVal) % mod;
            }
            return (int)result;
        }
    }
}
