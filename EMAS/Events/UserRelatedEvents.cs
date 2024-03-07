using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Events
{
    public static class UserRelatedEvents
    {

        public static event Action ChangePassword;
        public static event Action AddEmployee;
        public static event Action EditEmployee;

    }
}
