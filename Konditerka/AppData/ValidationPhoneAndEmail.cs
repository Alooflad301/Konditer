using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Konditerka.AppData
{
    public class ValidationPhoneAndEmail
    {
        public static bool IsValidPhone(string phone)
        {
            string clean = Regex.Replace(phone ?? "", @"[^\d]", "");
            return Regex.IsMatch(clean, @"^(?:\+?7|8|7)?[0-9]{10}$") && clean.Length == 11;
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
        }
    }
}
