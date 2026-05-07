using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konditerka.AppData
{
    public class CurrentUser
    {
        public static Users User { get; set; }
        public static bool IsAdmin => User?.Roles?.IdRole == 2;
    }
}
