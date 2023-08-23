using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YabraaEF.Const
{
    public static class General
    {
        public static DateTime GetKSATimeZoneNow()
        {
            TimeZoneInfo saudiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");
            DateTime saudiDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, saudiTimeZone);
            saudiDateTime = saudiDateTime.AddHours(-1);
            return saudiDateTime;
        }
    }
}
