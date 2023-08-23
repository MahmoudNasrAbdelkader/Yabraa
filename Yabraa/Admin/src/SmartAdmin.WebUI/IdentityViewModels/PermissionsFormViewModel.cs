using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartAdmin.WebUI.ViewModel
{
    public class PermissionsFormViewModel
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public List<CheckBoxViewModel> RoleCalims { get; set; }
    }
}
