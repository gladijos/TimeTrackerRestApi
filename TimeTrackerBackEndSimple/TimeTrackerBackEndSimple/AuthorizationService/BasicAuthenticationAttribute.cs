using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace TimeTrackerBackEndSimple
{
    public class BasicAuthenticationAttribute : AuthorizationFilterAttribute
    {
        public enum AuthorizationRoles
        {
            userAuthorizationRole = 0,
            adminAuthorizationRole = 1,
            AnonimousAuthorizationRole = 2
        }
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Authorization == null)
            {
                actionContext.Response = actionContext.Request
                    .CreateResponse(HttpStatusCode.Unauthorized);
            }
            else
            {
                string authenticationToken = actionContext.Request.Headers.Authorization.Parameter;
                string decodedAutheticationToken = Encoding.UTF8.GetString( Convert.FromBase64String(authenticationToken));
                string[] usernamePasswordArray = decodedAutheticationToken.Split(':');
                string username = usernamePasswordArray[0];
                string password = usernamePasswordArray[1];
                if (UserSecurity.Login(username, password))
                {
                    Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(username), null);
                }
                else
                {
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }
            //base.OnAuthorization(actionContext);
        }
    }
}