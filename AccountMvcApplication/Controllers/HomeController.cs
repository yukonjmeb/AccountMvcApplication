using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Common.Helper;
using System.IO;
using Common;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using AccountMvcApplication.Models;
using System.Web.Http;

namespace AccountMvcApplication.Controllers
{
    public class HomeController : Controller
    {
        private IWebApiHelper _webApiHelper;
        public HomeController(IWebApiHelper webapihelper)
        {
            this._webApiHelper = webapihelper;
        }

        string result = string.Empty;

        public ActionResult Index()
        {
            ViewBag.Title = "HomePage";
            return View();
        }

        public ActionResult Manage()
        {
            ViewBag.Title = "Manage";

            //改成串WebApi
            var UserList = _webApiHelper.getData("GetAccounts", null);
            var UserListObj = JsonConvert.DeserializeObject<List<LoginModel>>(UserList);

            //修改
            return View(UserListObj);
        }

        public ActionResult EditAccount(int id, bool IsConfirmed)
        {

            //改成串WebApi取得帳號資訊
            var UserInfo = _webApiHelper.getData("GetAccount", id.ToString());
            var UserInfoObj = ObjectParserHelper.JsonToObject<AccountModels>(UserInfo);

            var accountObj = new AccountModels()
            {
                UserID = id,
                UserName = UserInfoObj.UserName,
                PassWord = UserInfoObj.PassWord,
                IsConfirmed = IsConfirmed
            };

            bool changeAccountSucceeded = false;

            //改成串WebApi 修改密碼 
            bool.TryParse(_webApiHelper.PostData(Method.PUT, accountObj, "http://localhost:1322/api/Account/PutAccount/" + id.ToString()), out changeAccountSucceeded);

            return RedirectToAction("Manage");
        }

        public ActionResult DeleteAccount(int id)
        {

            bool deleteAccountSucceeded = false;
            //串WebApi 刪除資料
            bool.TryParse(_webApiHelper.PostData(Method.DELETE, null, "http://localhost:1322/api/Account/DeleteAccount/" + id.ToString()), out deleteAccountSucceeded);

            return RedirectToAction("Manage");
        }

        public ActionResult ShowAccountDetail(int id)
        {
            ViewBag.Title = id;

            //串WebApi取得資料
            var UserInfo = _webApiHelper.getData("GetAccount", id.ToString());
            var UserInfoObj = ObjectParserHelper.JsonToObject<EditAccountModel>(UserInfo);

            return PartialView("_LoadAccount", UserInfoObj);
        }


        public ActionResult Contact()
        {
            ViewBag.Title = "Contact";
            return View();
        }

        public IView UserListObj { get; set; }
    }
}
