using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using TimeTrackerBackEndSimple.Models;

namespace TimeTrackerBackEndSimple
{
    public class UserSecurity
    {
        public static bool Login (string username, string password)
        {
            using (TimeTrackerDBEntities entities = new TimeTrackerDBEntities())
            {
                return entities.Users.Any(user => user.UserName.Equals(username, StringComparison.OrdinalIgnoreCase)
                && user.Password == password);
            }
        }
        public static User GetCurrentAuthorizedUser()
        {
            string username = Thread.CurrentPrincipal.Identity.Name;
            User authorizedUser;
            using (TimeTrackerDBEntities entities = new TimeTrackerDBEntities())
            {
                authorizedUser = entities.Users.First(userIt => userIt.UserName.Equals(username));
            }
            return authorizedUser;
        }
        public static bool IsAdmin(User user)
        {
            return user.RoleId.Equals(BasicAuthenticationAttribute.AuthorizationRoles.adminAuthorizationRole);
        }
    }
}