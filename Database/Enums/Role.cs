using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Enums
{
    public enum Role
    {
        [Description("admin")]
        ADMIN,
        [Description("user")]
        USER
    }
}
