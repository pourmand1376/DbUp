using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DbUp.Helpers
{
   public static class HashCalculator
    {
        public static string MD5(this string s)
        {
            using (var provider = System.Security.Cryptography.MD5.Create())
            {
                StringBuilder builder = new StringBuilder();                           

                foreach (byte b in provider.ComputeHash(Encoding.UTF8.GetBytes(s)))
                    builder.Append(b.ToString("x2").ToLower());

                return builder.ToString();
            }
        }
    }
}
