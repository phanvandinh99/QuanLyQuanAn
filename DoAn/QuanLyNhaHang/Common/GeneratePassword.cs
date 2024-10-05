using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuanLyNhaHang.Common
{
    public class GeneratePassword
    {
        private static Random random = new Random();

        // Hàm sinh 6 số ngẫu nhiên
        public static string NewPassword(int length = 6)
        {
            var randomNumber = new char[length];
            for (int i = 0; i < length; i++)
            {
                randomNumber[i] = (char)('0' + random.Next(0, 10));
            }
            return new string(randomNumber);
        }
    }
}