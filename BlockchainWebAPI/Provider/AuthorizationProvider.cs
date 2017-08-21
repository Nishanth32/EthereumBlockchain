using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace BlockchainWebAPI.Provider
{
    public class AuthorizationProvider
    {
    }


    public class CustomAuthorize :AuthorizeAttribute
    {

        public override Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {

            if(actionContext.Request.Headers.Authorization==null|| actionContext.Request.Headers.Authorization.Parameter!="password")
            {
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }

            return base.OnAuthorizationAsync(actionContext, cancellationToken);
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Authorization == null || actionContext.Request.Headers.Authorization.Parameter != "password")
            {
                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }
          
        }

    }
}