using System;
using StackExchange.Redis;

namespace redis_set
{
    internal class Login
    {
        public string JwtID { get; set; }
        public DateTime UltimoAcesso { get; set; }
    }
}