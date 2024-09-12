using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_Chat
{
    internal class User
    {
        public string UserName { get; set; }
        public string PublicKey { get; set; }

        public User(string userName, string publicKey)
        {
            UserName = userName;
            PublicKey = publicKey;
        }
    }
}
