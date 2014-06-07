using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using UM.Model;
using UM.Interfaces;
using UM.Biz.Factory;
using UserManagementUI.ViewData.UserViewData;
using S1.Common.Model;
using SO.Provider;
using System.Configuration;
using S1.CommonBiz;
using SOProvider;

namespace UserManagementUI.Controllers
{
    public class UserController : BaseController
    {
        //This will rewrite the url according to IIS setting.
        #region Member Variables ---------------------------------
        UserProfileViewData _objUserProfileViewData;
        UserInfoViewData objUserInfoViewData;
        IUserInfo objIUserInfo;
        IRoleInfo objIRoleInfo;
        IWorkingUnit objIWorkingUnit;
        ISecuritySetting objISecuritySetting;
        //EmployeeService.EmployeeService _objService;//Baki.............
        bool IsActiveWebService =Convert.ToBoolean(ConfigurationManager.AppSettings["IsWebServiceActive"]);
        #endregion -------------------------------Member Variables

        #region Constructors -------------------------------------

        /// <summary>
        /// Constructor Initialize by object
        /// So after initialization objects will be available
        /// </summary>
        public UserController()
        {
            objUserInfoViewData = new UserInfoViewData();
            objUserInfoViewData.UserInfoList = new List<UserInfo>();
            objUserInfoViewData.UserInfo = new UserInfo();
            objUserInfoViewData.WorkingUnit = new WorkingUnit();
            objUserInfoViewData.UserInfo.UserRoles = new List<UserInRole>();
            objUserInfoViewData.UserRoleList = new List<RoleInfo>();
            objUserInfoViewData.UserInRole = new UserInRole();
            //Dynamically initializing Security BLL
            objIRoleInfo = SecurityFactory.InitiateRoleInfo();
            objIWorkingUnit = SecurityFactory.InitiateWorkingUnit();
            objISecuritySetting = SecurityFactory.InitiateSecuritySetting();
            objIUserInfo = SecurityFactory.InitiateUserInfo();
        }

        #endregion ------------------------------------Constractor

        #region ActionResults ------------------------------------

        #region UserInfo -----------------------------------------

        /// <summary>
        /// Get UserList to show in grid
        /// </summary>
        /// <returns></returns>
        public ActionResult UserList(string UserName)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                objUserInfoViewData.UserInfoList = objIUserInfo.GetUsers(string.Empty);
            }
            else
            {
                objUserInfoViewData.UserInfoList = objIUserInfo.GetUsers(UserName);
            }
            return View("UserList", objUserInfoViewData);
        }

        /// <summary>
        /// Get UserInfo by name
        /// Called to show user information after create
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public ActionResult GetUserByName(string UserName, string create)
        {
            objUserInfoViewData.UserInfo.UserName = UserName;
            objUserInfoViewData.UserInfo.Password = Session["Password"] as string;
            if (!string.IsNullOrEmpty(create))
            {
                ViewData["AlertMessage"] = GetMessageScript(SAVE);
            }
            //objUserInfoViewData.UserInfo = objIUserInfo.GetUserByName(UserName);            
            //objUserInfoViewData.WorkingUnit = objIWorkingUnit.GetWorkingUnitByUnitCode(objUserInfoViewData.UserInfo.WorkingUnitCode);
            //objUserInfoViewData.UserRoleList = objIRoleInfo.GetUserRoleList(objUserInfoViewData.UserInfo.UserID_PK);
            if (!string.IsNullOrEmpty(objUserInfoViewData.UserInfo.UserID_PK))
            {
                if (objUserInfoViewData.UserInfo.IsLokedOut)
                {
                    objUserInfoViewData.UserInfo.IsLokedOut = false;
                }
                else
                {
                    objUserInfoViewData.UserInfo.IsLokedOut = true;
                }
            }
            return View("NewUser", objUserInfoViewData);
        }

        /// <summary>
        /// Find User by name
        /// </summary>
        /// <param name="UserName"></param>
        /// <returns></returns>
        public ActionResult GetUserById(string UserId)
        {
            objUserInfoViewData.UserInfo = objIUserInfo.GetUserByID(UserId);
            objUserInfoViewData.WorkingUnit = objIWorkingUnit.GetWorkingUnitByUnitCode(objUserInfoViewData.UserInfo.WorkingUnitCode);
            return View("UserList", objUserInfoViewData);
        }

        /// <summary>
        /// Redirect to new user form
        /// Get application role to load role in UI
        /// </summary>
        /// <returns></returns>
        public ActionResult NewUser(string message, string UserId, string update)
        {
            //Get user if hidden field value is not empty 
            if (!string.IsNullOrEmpty(message))
            {
                ViewData["AlertMessage"] = GetMessageScript(message.Replace("\"","-"));
            }
            if (!string.IsNullOrEmpty(UserId))
            {
                objUserInfoViewData.UserInfo = objIUserInfo.GetUserByID(UserId);
                if (!string.IsNullOrEmpty(objUserInfoViewData.UserInfo.UserID_PK))
                {
                    if (objUserInfoViewData.UserInfo.IsLokedOut)
                    {
                        objUserInfoViewData.UserInfo.IsLokedOut = false;
                    }
                    else
                    {
                        objUserInfoViewData.UserInfo.IsLokedOut = true;
                    }
                }

            }
            //Get userrole 
            if (objUserInfoViewData.UserInfo.UserRoles == null)
            {
                objUserInfoViewData.UserInfo.UserRoles = new List<UserInRole>();
            }
            return View("NewUser", objUserInfoViewData);
        }

        /// <summary>
        /// Save data to DB 
        /// </summary>
        /// <param name="objForm"></param>
        /// <returns></returns>
        public ActionResult SaveUser([Bind()] UserInfo objUserInfo, FormCollection objFrm, string UserId)
        {
            ValidateModelState(objUserInfo);
            
           
                if (ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(UserId) && string.IsNullOrEmpty(objFrm["Roles"]))
                    {
                        objUserInfoViewData.UserInfo.ErrorMessage_VW = "Please assing at least one role";
                    }
                    else
                    {
                        objUserInfo.UserID_PK = UserId;
                        if (String.IsNullOrEmpty(UserId))
                        {
                            objUserInfo.IsNew = true;
                        }
                        else
                        {
                            objUserInfo.IsNew = false;
                        }
                        objUserInfo = (UserInfo)SetObjectStatus(objUserInfo);
                        //Assign User role  
                        string Roles = objFrm["Roles"].TrimStart('_').ToString();
                        string[] RoleIDs = Roles.Split('_');

                        List<UserInRole> objUserRoles = new List<UserInRole>();
                        UserInRole objUserInRole;
                        for (int i = 0; i < RoleIDs.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(RoleIDs[i]))
                            {
                                objUserInRole = new UserInRole();
                                objUserInRole.UserRoleID_PK = Guid.NewGuid().ToString();
                                objUserInRole.RoleID = RoleIDs[i];
                                objUserRoles.Add(objUserInRole);
                            }
                        }
                        objUserInfo.UserRoles = objUserRoles;
                        string ReturnMessage = objIUserInfo.SaveUser(objUserInfo);
                        //Update Employee 
                        if (IsActiveWebService && ReturnMessage == SAVE)
                        {
                            //LS.Security.UI.EmployeeService.CommonEmployee objCommonEmployee = new LS.Security.UI.EmployeeService.CommonEmployee();
                            //objCommonEmployee.EmployeeCode_PK = objFrm["EmployeeCode"].ToString();
                            //objCommonEmployee.EmployeeUserCode = objUserInfo.UserID_PK;
                            //ReturnMessage = UpdateEmployeeInfo(objCommonEmployee);//Baki................
                        }
                        if (string.IsNullOrEmpty(ReturnMessage))
                        {
                            Session["Password"] = objUserInfo.Password;
                            if (string.IsNullOrEmpty(objUserInfo.UserID_PK))
                            {
                                return RedirectToAction("GetUserByName", new { @UserName = objUserInfo.UserName, @create = "success" });
                            }
                            else
                            {
                                return RedirectToAction("NewUser", new { @update = "success" });
                            }
                        }
                        else
                        {
                            ReturnMessage = GetExcecutionMessage(ReturnMessage, SAVE);
                            return RedirectToAction("NewUser", new { @message = ReturnMessage });
                        }
                    }
                }
            
            return View("NewUser", objUserInfoViewData);
        }

        /// <summary>
        /// Redirest to Reset Password window/view
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public ActionResult ResetPassword(string UserId)
        {
            objUserInfoViewData.UserInfo = objIUserInfo.GetUserByID(UserId);
            objUserInfoViewData.UserInfo.Password = string.Empty;
            return View("ResetPassword", objUserInfoViewData);
        }

        /// <summary>
        /// Reset UserPassword
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult SaveResetPassword(string UserId, string UserName)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                ModelState.AddModelError("UserName", MANDATORY);
            }
            if (ViewData.ModelState.IsValid)
            {
                UserInfo oUser = objIUserInfo.GetUserByName(UserName);
                if (!string.IsNullOrEmpty(oUser.UserID_PK))
                {
                    string NewPassword = objIUserInfo.ResetPassword(UserName);
                    objUserInfoViewData.UserInfo = objIUserInfo.GetUserByID(UserId);
                    objUserInfoViewData.UserInfo.Password = NewPassword;
                    if (!string.IsNullOrEmpty(NewPassword))
                    {
                        ViewData["AlertMessage"] = GetMessageScript(SAVE);
                    }
                }
            }
            return View("ResetPassword", objUserInfoViewData);
        }

        /// <summary>
        /// Redirest to Change Password window/view
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public ActionResult ChangePassword(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                ViewData["AlertMessage"] = "<div class='error'>" + message + "</div>";
            }
            string PasswordRequiredValue = "Password will contains any of the character from : " + "\"" + objISecuritySetting.GetListSecuritySetting()[0].NonAlphaNumChar + "\"";
            PasswordRequiredValue += "</br> Password will contains minimum " + objISecuritySetting.GetListSecuritySetting()[0].MinReqPassLen + " chanracters";
            ViewData["NonAlphaNumChar"] = PasswordRequiredValue;
            return View("ChangePassword", objUserInfoViewData);
        }

        /// <summary>
        /// Save change password
        /// User can change own passowrd
        /// </summary>
        /// <param name="objForm"></param>
        /// <returns></returns>
        public ActionResult SaveChangePassword(FormCollection objForm)
        {
            if (string.IsNullOrEmpty(objForm["OldPassword"].ToString()))
            {
                ModelState.AddModelError("Password", MANDATORY);
            }
            if (string.IsNullOrEmpty(objForm["NewPassword"].ToString()))
            {
                ModelState.AddModelError("Password", MANDATORY);
            }
            if (string.IsNullOrEmpty(objForm["ConfirmPassword"].ToString()))
            {
                ModelState.AddModelError("Password", MANDATORY);
            }
            if (objForm["NewPassword"].ToString() != objForm["ConfirmPassword"].ToString())
            {
                ModelState.AddModelError("Password", "Password is not valid.");
            }
            if (ViewData.ModelState.IsValid)
            {
                UserInfo objUserInfo = objIUserInfo.GetUserByID(SessionUtility.SessionContainer.USER_ID);
                string ReturnMessage = objIUserInfo.ChangePassword(objUserInfo.UserName, objForm["OldPassword"].ToString(), objForm["NewPassword"].ToString());
                if (string.IsNullOrEmpty(ReturnMessage))
                {
                    ViewData["AlertMessage"] = GetMessageScript(SAVE);
                    SOSession.RequiredChangePassword = false;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("ChangePassword", new { @message = ReturnMessage });
                }

            }
            return View("ChangePassword", objUserInfoViewData);
        }

        /// <summary>
        /// Redirect to new user view to edit user information
        /// Auto fill all existing information 
        /// </summary>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public ActionResult EditUser(string UserId)
        {
            objUserInfoViewData.UserInfo = objIUserInfo.GetUserByID(UserId);
            objUserInfoViewData.WorkingUnit = objIWorkingUnit.GetWorkingUnitByUnitCode(objUserInfoViewData.UserInfo.WorkingUnitCode);
            if (objUserInfoViewData.UserInfo.IsLokedOut)
            {
                objUserInfoViewData.UserInfo.IsLokedOut = false;
            }
            else
            {
                objUserInfoViewData.UserInfo.IsLokedOut = true;
            }
            objUserInfoViewData.UserRoleList = objIRoleInfo.GetUserRoleList(UserId);
            return View("NewUser", objUserInfoViewData);
        }

        /// <summary>
        /// Get all application role 
        /// </summary>
        /// <returns></returns>
        public ActionResult GetAssignRoleList(string UserId)
        {
            objUserInfoViewData.UserRoleList = objIRoleInfo.GetRoles(string.Empty);
            objUserInfoViewData.UserInfo = objIUserInfo.GetUserByID(UserId);
            if (objUserInfoViewData.UserInfo.UserRoles == null)
            {
                objUserInfoViewData.UserInfo.UserRoles = new List<UserInRole>();
            }
            return View("AssignRolePopup", objUserInfoViewData);
        }

        /// <summary>
        /// Redirect to Login view
        /// </summary>
        /// <returns></returns>
        public ActionResult LogIn()
        {
            return View("LogIn", objUserInfoViewData);
        }

        /// <summary>
        /// Clear all session
        /// Logout from application and redirect to login view
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOut()
        {
            Session.Clear();
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.Now);
            SOSession.Logout();
            return View("LogIn", objUserInfoViewData);
        }

        /// <summary>
        /// Authenticate user and redirect to UserList
        /// </summary>
        /// <param name="objUserInfo"></param>
        /// <returns></returns>
        [OutputCache(Duration = 1, VaryByParam = "*")]
        public ActionResult AuthenticateUser([Bind()] UserInfo objUserInfo)
        {
            if (string.IsNullOrEmpty(objUserInfo.UserName))
            {
                ModelState.AddModelError("UserName", MANDATORY);
            }
            if (string.IsNullOrEmpty(objUserInfo.Password))
            {
                ModelState.AddModelError("Password", MANDATORY);
            }
            if (ModelState.IsValid)
            {
                if (objIUserInfo.AuthenticateUser(objUserInfo.UserName, objUserInfo.Password))
                {

                    SORoleProvider objSORoleProvider = new SORoleProvider();
                    if (objSORoleProvider.IsUserInRole(objUserInfo.UserName, "SuperAdmin") || objSORoleProvider.IsUserInRole(objUserInfo.UserName, "SysAdmin"))
                    {
                        Session["CurrentUser"] = objUserInfo.UserName;
                        if (SOSession.RequiredChangePassword && HttpContext.User.Identity.Name != SecurityConstant.SuperUserName)
                        {
                            return RedirectToAction("ChangePassword");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        Session.Clear();
                        Response.Cache.SetCacheability(HttpCacheability.NoCache);
                        Response.Cache.SetExpires(DateTime.Now);
                        SOSession.Logout();
                        string ErrorText = "<div class='error'> User has no previllege to enter into system.</div>";
                        ViewData["AlertMessage"] = ErrorText;
                        return View("LogIn", objUserInfoViewData);
                    }
                }
                else
                {
                    string ErrorText = "<div class='error'> UserName or Password is not corrent. Please varify UserName and Password.</div>";
                    ViewData["AlertMessage"] = ErrorText;
                    return View("LogIn", objUserInfoViewData);
                }
            }
            else
            {
                return View("LogIn", objUserInfoViewData);
            }
        }

        #endregion ---------------------------------------UserInfo


        #region User Profile -------------------------------------
        private void InitialUserProfileViewData()
        {
            _objUserProfileViewData = new UserProfileViewData();
            _objUserProfileViewData.UserProfile = new UserProfile();
            _objUserProfileViewData.UserProfileList = new List<UserProfile>();

            //_objService = new EmployeeService.EmployeeService();
            //EmployeeService.AuthHeader authentication = new EmployeeService.AuthHeader();
            //authentication.LoginTocken = "leadsoft";// SessionUtility.SessionContainer.USER_ID;          
            //_objService.AuthHeaderValue = authentication; 
        }

        /// <summary>
        /// Get Employee Information as User Profile
        /// </summary>
        /// <param name="pEmployeeUserCode"></param>
        /// <returns></returns>   
        [OutputCache(Duration = 1, VaryByParam = "*")]
        public ActionResult GetEmployeeProfile(string pEmployeeUserCode)
        {
            InitialUserProfileViewData();
            if (IsActiveWebService)
            {
                //List<EmployeeService.CommonEmployee> objEmployeeList = new List<EmployeeService.CommonEmployee>();
                //EmployeeService.CommonEmployee objEmployee = new EmployeeService.CommonEmployee();
                //if (!string.IsNullOrEmpty(pEmployeeUserCode))
                //{
                //    objEmployee = _objService.GetEmployee(pEmployeeUserCode);
                //    objEmployeeList.Add(objEmployee);
                //    _objUserProfileViewData.UserProfile = ConvertCommonEmployeeToUserProfile(objEmployeeList)[0];
                //}
            }
            return View("UserProfile", _objUserProfileViewData);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCompanyId"></param>
        /// <param name="pWorkingUnitId"></param>
        /// <returns></returns>
        [OutputCache(Duration = 1, VaryByParam = "*")]
        public ActionResult GetUserProfileListPopup(int pCompanyId, int pWorkingUnitId)
        {
            InitialUserProfileViewData();

            if (IsActiveWebService)
            {
                //List<EmployeeService.CommonEmployee> objEmployeeList = new List<EmployeeService.CommonEmployee>();
                //if (pWorkingUnitId > 0)
                //{
                //    objEmployeeList = _objService.GetEmployeeList(pCompanyId, pWorkingUnitId).ToList();
                //    _objUserProfileViewData.UserProfileList = ConvertCommonEmployeeToUserProfile(objEmployeeList);
                //}
                //else
                //{
                //    ViewData["AlertMessage"] = GetMessageScript("WorkingUnitId is empty.");
                //}
            }
            return View("UserProfileList", _objUserProfileViewData);
        }

        /// <summary>
        /// Convert Common Employee to User's Profile
        /// </summary>
        /// <param name="oblEmployeeList"></param>
        /// <returns></returns>
        //private List<UserProfile> ConvertCommonEmployeeToUserProfile(List<EmployeeService.CommonEmployee> oblEmployeeList)
        //{
        //    List<UserProfile> objUserProfileList = new List<UserProfile>();
        //    UserProfile objUserProfile = null;
        //    EmployeeService.CommonEmployee objCommonEmployee = null;
        //    if (oblEmployeeList != null && oblEmployeeList.Count > 0)
        //    {
        //        for (int i = 0; i < oblEmployeeList.Count; i++)
        //        {
        //            objCommonEmployee = oblEmployeeList[i];
        //            objUserProfile = new UserProfile();
        //            objUserProfile.EmployeeCode = objCommonEmployee.EmployeeCode_PK;
        //            objUserProfile.UserProfileId = objCommonEmployee.EmployeeId;
        //            objUserProfile.FullName = objCommonEmployee.Name;
        //            objUserProfile.Sex_VW = objCommonEmployee.Sex_VW;
        //            objUserProfile.FatherName = objCommonEmployee.FatherName;
        //            objUserProfile.MotherName = objCommonEmployee.MotherName;
        //            objUserProfile.JoinningDate = objCommonEmployee.JoinningDate;
        //            objUserProfile.ConfirmationDate = objCommonEmployee.ConfirmationDate;
        //            objUserProfile.JobStatus = objCommonEmployee.JobStatus;
        //            objUserProfile.PresentAddress = objCommonEmployee.PresentAddress;
        //            objUserProfile.PermanentAddress = objCommonEmployee.PermanentAddress;
        //            objUserProfile.PreAddDistrict_VW = objCommonEmployee.PreAddDistrict_VW;
        //            objUserProfile.ResidencePhone = objCommonEmployee.ResidencePhone;
        //            objUserProfile.CellPhone = objCommonEmployee.CellPhone;
        //            objUserProfile.Email = objCommonEmployee.Email;
        //            objUserProfile.PassportNo = objCommonEmployee.PassportNo;
        //            objUserProfile.DateOfBirth = objCommonEmployee.DateOfBirth;
        //            objUserProfile.NationalityId = objCommonEmployee.NationalityId;
        //            objUserProfile.TinNo = objCommonEmployee.TinNo;
        //            objUserProfile.Department_VW = objCommonEmployee.Department_VW;
        //            objUserProfile.Designation_VW = objCommonEmployee.Designation_VW;
        //            objUserProfile.EmploymentType_VW = objCommonEmployee.EmploymentType_VW;
        //            objUserProfile.MaritalStatus_VW = objCommonEmployee.MaritalStatus_VW;
        //            objUserProfile.BloodGroup_VW = objCommonEmployee.BloodGroup_VW;
        //            objUserProfile.Religion_VW = objCommonEmployee.Religion_VW;
        //            objUserProfile.Nationality_VW = objCommonEmployee.Nationality_VW;
        //            objUserProfile.JoinBatch_VW = objCommonEmployee.JoinBatch_VW;
        //            objUserProfile.Company_VW = objCommonEmployee.Company_VW;
        //            objUserProfileList.Add(objUserProfile);
        //        }
        //    }
        //    return objUserProfileList;
        //}

        /// <summary>
        /// Convert User's Profile to Common Employee 
        /// </summary>
        /// <param name="objUserProfile"></param>
        /// <returns></returns>
        //private EmployeeService.CommonEmployee ConvertUserProfileToCommonEmployee(UserProfile objUserProfile)
        //{
        //    EmployeeService.CommonEmployee objCommonEmployee = new EmployeeService.CommonEmployee();
        //    if (objUserProfile != null)
        //    {

        //        objCommonEmployee.EmployeeUserCode = objUserProfile.UserId;
        //        objCommonEmployee.EmployeeId = objUserProfile.UserProfileId;
        //        objCommonEmployee.Name = objUserProfile.FullName;
        //        objCommonEmployee.Sex_VW = objUserProfile.Sex_VW;
        //        objCommonEmployee.FatherName = objUserProfile.FatherName;
        //        objCommonEmployee.MotherName = objUserProfile.MotherName;
        //        objCommonEmployee.JoinningDate = objUserProfile.JoinningDate;
        //        objCommonEmployee.ConfirmationDate = objUserProfile.ConfirmationDate;
        //        objCommonEmployee.JobStatus = objUserProfile.JobStatus;
        //        objCommonEmployee.PresentAddress = objUserProfile.PresentAddress;
        //        objCommonEmployee.PermanentAddress = objUserProfile.PermanentAddress;
        //        objCommonEmployee.PreAddDistrict_VW = objUserProfile.PreAddDistrict_VW;
        //        objCommonEmployee.ResidencePhone = objUserProfile.ResidencePhone;
        //        objCommonEmployee.CellPhone = objUserProfile.CellPhone;
        //        objCommonEmployee.Email = objUserProfile.Email;
        //        objCommonEmployee.PassportNo = objUserProfile.PassportNo;
        //        objCommonEmployee.DateOfBirth = objUserProfile.DateOfBirth;
        //        objCommonEmployee.NationalityId = objUserProfile.NationalityId;
        //        objCommonEmployee.TinNo = objUserProfile.TinNo;
        //        objCommonEmployee.Department_VW = objUserProfile.Department_VW;
        //        objCommonEmployee.Designation_VW = objUserProfile.Designation_VW;
        //        objCommonEmployee.EmploymentType_VW = objUserProfile.EmploymentType_VW;
        //        objCommonEmployee.MaritalStatus_VW = objUserProfile.MaritalStatus_VW;
        //        objCommonEmployee.BloodGroup_VW = objUserProfile.BloodGroup_VW;
        //        objCommonEmployee.Religion_VW = objUserProfile.Religion_VW;
        //        objCommonEmployee.Nationality_VW = objUserProfile.Nationality_VW;
        //        objCommonEmployee.JoinBatch_VW = objUserProfile.JoinBatch_VW;
        //        objCommonEmployee.Company_VW = objUserProfile.Company_VW;
        //    }
        //    return objCommonEmployee;
        //}

        /// <summary>
        /// Update Employee Information by UserCode
        /// </summary>
        /// <param name="objCommonEmployee"></param>
        /// <returns></returns>
        //private string UpdateEmployeeInfo(EmployeeService.CommonEmployee objCommonEmployee)
        //{
        //    InitialUserProfileViewData();
        //    string vResult = string.Empty;           
        //    if (!string.IsNullOrEmpty(objCommonEmployee.EmployeeUserCode))
        //    {
        //        vResult = _objService.UpdateEmployee(objCommonEmployee.EmployeeCode_PK, objCommonEmployee.EmployeeUserCode);
        //    }
        //    return vResult;
        //}
        #endregion -----------------------------------User Profile

        #endregion ----------------------------------Actionresults

        #region Methods ------------------------------------------
        /// <summary>
        /// Check required fields
        /// </summary>
        /// <param name="objUserInfo"></param>
        /// <param name="UserId"></param>
        private void ValidateModelState(UserInfo objUserInfo)
        {
            if (string.IsNullOrEmpty(objUserInfo.UserName.Trim().ToString()))
            {
                ModelState.AddModelError("UserName", MANDATORY);
            }
            if (string.IsNullOrEmpty(objUserInfo.Email.Trim().ToString()))
            {
                ModelState.AddModelError("Email", MANDATORY);
            }
            if (objUserInfo.WorkingUnitCode <= 0)
            {
                ModelState.AddModelError("WorkingUnitCode", MANDATORY);
            }
        }

        #endregion ----------------------------------------Methods
    }
}
