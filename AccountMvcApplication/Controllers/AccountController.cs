using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using AccountMvcApplication.Filters;
using AccountMvcApplication.Models;
using Common;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Common.Helper;
using System.Net.Http;

namespace AccountMvcApplication.Controllers
{
    //[Authorize]
    //[InitializeSimpleMembership]
    public class AccountController : Controller
    {

        private IWebApiHelper _webApiHelper;
        public AccountController(IWebApiHelper webapihelper)
        {
            this._webApiHelper = webapihelper;
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            //改成串WebApi登入帳號
            var UserInfo = _webApiHelper.getData("GetAccountByName", model.UserName);
            var UserInfoObj = ObjectParserHelper.JsonToObject<LoginModel>(UserInfo);

            if (UserInfoObj != null)
            {
                if (ModelState.IsValid && UserInfoObj.UserName == model.UserName.Trim() && UserInfoObj.Password == model.Password.Trim())
                    {
                            if (UserInfoObj.IsConfirmed == false)
                            {
                                ModelState.AddModelError("", "登入失敗，帳戶已停用");
                            }else{
                            Session["UserName"] = model.UserName;
                            return RedirectToLocal(returnUrl);
                            }
                    }
                    else
                    {
                        // 如果執行到這裡，發生某項失敗，則重新顯示表單
                        ModelState.AddModelError("", "所提供的使用者名稱或密碼不正確。");
                    }
            }
            else {
                // 如果執行到這裡，發生某項失敗，則重新顯示表單
                ModelState.AddModelError("", "查無此帳戶。");
            }

            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            Session.Remove("UserName");
 
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // 嘗試註冊使用者
                try
                {
                    //改寫為串WEBAPI新增帳戶
                    //POST 傳入參數
                    var accountObj = new AccountModels()
                    {
                        UserName = model.UserName,
                        PassWord = model.Password,
                        IsConfirmed = true
                    };

                    //抓出資料庫資料
                    var UserInfo = _webApiHelper.getData("GetAccountByName", model.UserName);

                    if (UserInfo != "null")
                    {
                        // 如果執行到這裡，發生某項失敗，則重新顯示表單
                        ModelState.AddModelError("", "您所輸入的使用者名稱已存在。");
                    }
                    else {
                        var accountData = _webApiHelper.PostData(Method.POST, accountObj, "http://localhost:1322/api/Account/PostAccount/");
                        var accountJson = ObjectParserHelper.JsonToObject<LoginModel>(accountData);
                        Session["UserName"] = accountJson.UserName;
                        return RedirectToAction("Index", "Home");
                    }
                    
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // 如果執行到這裡，發生某項失敗，則重新顯示表單
            return View(model);
        }

        //
        // GET: /Account/Manage
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "您的密碼已變更。"
                : message == ManageMessageId.SetPasswordSuccess ? "已設定您的密碼。"
                : message == ManageMessageId.RemoveLoginSuccess ? "已移除外部登入。"
                : "";
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            //改成串WebApi取得帳號資訊
            var UserInfo = _webApiHelper.getData("GetAccountByName", Session["UserName"].ToString());
            var UserInfoObj = ObjectParserHelper.JsonToObject<AccountModels>(UserInfo);

            ViewBag.ReturnUrl = Url.Action("Manage");

                if (ModelState.IsValid)
                {
                    // 在特定失敗狀況下，ChangePassword 會擲回例外狀況，而非傳回 false。
                    bool checkPassowrd;
                    bool changePasswordSucceeded =false;

                    //先驗證舊密碼是否正確
                    if (UserInfoObj.PassWord == model.OldPassword) { checkPassowrd = true; } else { checkPassowrd = false; }

                    if (checkPassowrd)
                    {
                        try
                        {
                            //POST 傳入參數id,account
                            var accountObj = new AccountModels()
                            {
                                UserID = UserInfoObj.UserID,
                                UserName = UserInfoObj.UserName,
                                PassWord = model.NewPassword,
                                IsConfirmed = UserInfoObj.IsConfirmed
                            };

                            //改成串WebApi 修改密碼 
                            bool.TryParse(_webApiHelper.PostData(Method.PUT, accountObj, "http://localhost:1322/api/Account/PutAccount/" + UserInfoObj.UserID), out changePasswordSucceeded);

                        }
                        catch (Exception)
                        {
                            changePasswordSucceeded = false;
                        }

                        if (changePasswordSucceeded)
                        {
                            return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                        }
                        else
                        {
                            ModelState.AddModelError("", "新密碼無效,修改密碼失敗。");
                        }
                    }
                    else {
                        ModelState.AddModelError("", "目前密碼不正確。");
                    }
                }

            // 如果執行到這裡，發生某項失敗，則重新顯示表單
            return View(model);
        }

 
        #region Helper
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // 請參閱 http://go.microsoft.com/fwlink/?LinkID=177550 了解
            // 狀態碼的完整清單。
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "使用者名稱已經存在。請輸入不同的使用者名稱。";

                case MembershipCreateStatus.DuplicateEmail:
                    return "該電子郵件地址的使用者名稱已經存在。請輸入不同的電子郵件地址。";

                case MembershipCreateStatus.InvalidPassword:
                    return "所提供的密碼無效。請輸入有效的密碼值。";

                case MembershipCreateStatus.InvalidEmail:
                    return "所提供的電子郵件地址無效。請檢查這項值，然後再試一次。";

                case MembershipCreateStatus.InvalidAnswer:
                    return "所提供的密碼擷取解答無效。請檢查這項值，然後再試一次。";

                case MembershipCreateStatus.InvalidQuestion:
                    return "所提供的密碼擷取問題無效。請檢查這項值，然後再試一次。";

                case MembershipCreateStatus.InvalidUserName:
                    return "所提供的使用者名稱無效。請檢查這項值，然後再試一次。";

                case MembershipCreateStatus.ProviderError:
                    return "驗證提供者傳回錯誤。請確認您的輸入，然後再試一次。如果問題仍然存在，請聯繫您的系統管理員。";

                case MembershipCreateStatus.UserRejected:
                    return "使用者建立要求已取消。請確認您的輸入，然後再試一次。如果問題仍然存在，請聯繫您的系統管理員。";

                default:
                    return "發生未知的錯誤。請確認您的輸入，然後再試一次。如果問題仍然存在，請聯繫您的系統管理員。";
            }
        }
        #endregion
    }
}
