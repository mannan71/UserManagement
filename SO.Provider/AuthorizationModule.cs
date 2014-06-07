using System;
using System.Security;
using System.Web;
using System.Web.Routing;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using System.Data;
using System.Collections.Generic;
using SO.Provider;

namespace SOProvider
{
    public class AuthorizationModule : IHttpModule
    {
        #region Constructor
        public AuthorizationModule()
        {
            // ctor
        }
        #endregion
        #region IHttpModule Members
        #region Dispose
        public void Dispose()
        {
            // Usually, nothing has to happen here till now...
        }
        #endregion
        #region Init
        public void Init(HttpApplication httpApp)
        {
            httpApp.AuthorizeRequest += new EventHandler(this.OnAuthorizeRequest);
        }
        #endregion
        #endregion
        #region Event Handlers
        /// <summary>
        /// On Authorize
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnAuthorizeRequest(object sender, EventArgs e)
        {
            bool isAuthorized = false;
            List<string> vActions = new List<string>();
            HttpContext context = ((HttpApplication)sender).Context;
            RouteData routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(context));
            string connectionStringKey = AuthorizationMapping.GetAuthorizationMappingSettings().ConnectionStringKey;
            string nameOfAnonymousRole = AuthorizationMapping.GetAuthorizationMappingSettings().NameOfAnonymousRole;

            if (routeData != null)
            {
                // avoid authorization of resourcde.axd -- for some reason the ignoreroute failed here
                if (routeData.Values["controller"] == null || routeData.Values["controller"] == "~")
                    return;

                string controller = routeData.GetRequiredString("controller");
                string action = routeData.GetRequiredString("action");

                if (!string.IsNullOrEmpty(SecurityConstant.RewriteURL))
                    action = action.Replace(SecurityConstant.RewriteURL, "");

                string actionName = controller + "/" + action + "?";

                string pModuleCode = string.Empty;
                string pFormCode = string.Empty;
                string pPageCode = string.Empty;
                string TransferPromotion = string.Empty;
                string pWorkingUnitCode = string.Empty;
                string pWorkingUnitName = string.Empty;
                //Actions which contains hardcode value with action name
                //For Report
                vActions.Add("GetReportCallingListById".ToUpper());
                //For General code file
                vActions.Add("GetGeneralCodeFileTypeAll".ToUpper());
                //For Transfer
                vActions.Add("CreateProcessTransferPromotion".ToUpper());
                //For Working Unit
                vActions.Add("CreateUnit".ToUpper());
                //For Dynamic controls
                vActions.Add("RenderingDynamicPageControls".ToUpper());
                //For ..................
                vActions.Add("OpenClientAccountInfo".ToUpper());
                //For Rating Setup..................
                vActions.Add("CreateRatingsAll".ToUpper());
                if (vActions.Contains(action.ToUpper()))
                //if (actionName.Contains("GetGeneralCodeFileTypeAll") || actionName.Contains("CreateProcessTransferPromotion") || actionName.Contains("CreateUnit") || actionName.Contains("RenderingDynamicPageControls") || actionName.Contains("OpenClientAccountInfo"))
                {
                    if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["pModuleCode"]))
                    {
                        pModuleCode = HttpContext.Current.Request.QueryString["pModuleCode"];
                        actionName += "pModuleCode=" + pModuleCode + "&";
                    }
                    if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["pFormCode"]))
                    {
                        pFormCode = HttpContext.Current.Request.QueryString["pFormCode"];
                        actionName += "pFormCode=" + pFormCode + "&";
                    }
                    if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["pPageCode"]))
                    {
                        pPageCode = HttpContext.Current.Request.QueryString["pPageCode"];
                        actionName += "pPageCode=" + pPageCode + "&";
                    }
                    if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["TransferPromotion"]))
                    {
                        TransferPromotion = HttpContext.Current.Request.QueryString["TransferPromotion"];
                        actionName += "TransferPromotion=" + TransferPromotion + "&";
                    }
                    if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["pWorkingUnitCode"]))
                    {
                        pWorkingUnitCode = HttpContext.Current.Request.QueryString["pWorkingUnitCode"];
                        actionName += "pWorkingUnitCode=" + pWorkingUnitCode + "&";
                    }
                    if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["pWorkingUnitName"]))
                    {
                        pWorkingUnitName = HttpContext.Current.Request.QueryString["pWorkingUnitName"];
                        actionName += "pWorkingUnitName=" + pWorkingUnitName + "&";
                    }
                    //For Report
                    if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["pReportCode"]))
                    {
                        string vReportCode = HttpContext.Current.Request.QueryString["pReportCode"];
                        actionName += "pReportCode=" + vReportCode + "&";
                    }
                    if (!string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["pReportID"]))
                    {
                        string vReportId = HttpContext.Current.Request.QueryString["pReportID"];
                        actionName += "pReportID=" + vReportId + "&";
                    }
                }

                actionName = actionName.Remove(actionName.Length - 1, 1);
               
                // Skip actions
                if (actionName.EndsWith(".css") || actionName.EndsWith(".jpg") || actionName.EndsWith(".axd") || actionName.Contains("CrystalImageHandler") || actionName.Contains("GetReportHolderUI"))
                {
                    isAuthorized = true;
                }
                if (string.IsNullOrEmpty(HttpContext.Current.User.Identity.Name.ToString()))
                {
                    isAuthorized = true;
                    return;
                }
                // get permittable roles for this action
                string actionInRoles = SOActionInRole.ActionIsInRole(actionName);
                string[] arrActionInRoles = actionInRoles.Split(',');
                for (int i = 0; i < arrActionInRoles.Length; i++)
                {
                    // check whether user has this role or this is allowed for anonymous role
                    if (SOUserInRole.UserIsInRole(arrActionInRoles[i]) || arrActionInRoles[i].Equals(nameOfAnonymousRole))
                    {
                        isAuthorized = true;

                        break;
                    }
                }

                //isAuthorized = true;

                // Activity Logging
                if (isAuthorized)
                {
                }
                if (!isAuthorized)
                {
                    throw new ApplicationException("Access denied. Permission on " + actionName + " not found");
                }
            }
        }

        /// <summary>
        /// Check a specific action has ermission for the user or not
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static bool CheckActionHasPermission(string controller, string action)
        {
            bool isAuthorized = false;
            string nameOfAnonymousRole = AuthorizationMapping.GetAuthorizationMappingSettings().NameOfAnonymousRole;

            string actionName = controller + "/" + action;

            // get permittable roles for this action
            string actionInRoles = SOActionInRole.ActionIsInRole(actionName);
            string[] arrActionInRoles = actionInRoles.Split(',');
            for (int i = 0; i < arrActionInRoles.Length; i++)
            {
                // check whether user has this role or this is allowed for anonymous role
                if (SOUserInRole.UserIsInRole(arrActionInRoles[i]) || arrActionInRoles[i].Equals(nameOfAnonymousRole))
                {
                    isAuthorized = true;

                    break;
                }
            }
            return isAuthorized;
        }

        #endregion
    }
}