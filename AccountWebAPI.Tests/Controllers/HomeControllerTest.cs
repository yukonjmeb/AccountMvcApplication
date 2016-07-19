using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AccountWebAPI.Controllers;
using System.Web.Mvc;

namespace AccountWebAPI.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

        }
    }
}
