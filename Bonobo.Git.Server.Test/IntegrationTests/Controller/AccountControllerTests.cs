using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Test.IntegrationTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using SpecsFor.Mvc;
using System;
using System.Linq;

namespace Bonobo.Git.Server.Test.Integration.Web
{
    using ITH = IntegrationTestHelpers;
    public class AccountControllerSpecs
    {
        [TestClass]
        public class AccountControllerTests
        {
            private static MvcWebApp app;

            [ClassInitialize]
            public static void ClassInit(TestContext testContext)
            {
                app = new MvcWebApp();
            }

            [ClassCleanup]
            public static void Cleanup()
            {
                app.Browser.Close();
            }

            [TestInitialize]
            public void InitTest()
            {
                IntegrationTestHelpers.Login(app);
            }

            [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
            public void UserDetailRequiresLogin()
            {
                app.NavigateTo<HomeController>(c => c.LogOff());
                app.UrlShouldMapTo<HomeController>(c => c.LogOn("/Home/Index"));

                app.NavigateTo<AccountController>(c => c.Detail(new Guid("7479fc09-2c0b-4e93-a2cf-5e4bbf6bab4f")));

                app.UrlShouldMapTo<HomeController>(c => c.LogOn("/Account/Detail/7479fc09-2c0b-4e93-a2cf-5e4bbf6bab4f"));
            }

            [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
            public void LoginWithoutCredentialFailsWithInvalidMessages()
            {
                app.NavigateTo<HomeController>(c => c.LogOn("/"));
                app.FindFormFor<LogOnModel>()
                    .Field(f => f.Username).SetValueTo("")
                    .Field(f => f.Password).SetValueTo("")
                    .Submit();

                app.FindFormFor<LogOnModel>()
                    .Field(f => f.Username).ShouldBeInvalid();
                app.FindFormFor<LogOnModel>()
                    .Field(f => f.Password).ShouldBeInvalid();
            }

            [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
            public void UsernameEnsureDuplicationDetectionAsYouTypeWorksOnCreation()
            {
                using (var id1 = ITH.CreateUsers(app, 1).Single())
                {
                    app.NavigateTo<AccountController>(c => c.Create());
                    var form = app.FindFormFor<UserCreateModel>()
                        .Field(f => f.Username).SetValueTo(id1.Username)
                        .Field(f => f.Name).Click(); // Set focus


                    var validation = app.WaitForElementToBeVisible(By.CssSelector("input#Username~span.field-validation-error>span"), TimeSpan.FromSeconds(1), true);
                    Assert.AreEqual(Resources.Validation_Duplicate_Name, validation.Text);

                    var input = app.Browser.FindElementByCssSelector("input#Username");
                    Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));
                }
            }

            [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
            public void UsernameEnsureDuplicationDetectionAsYouTypeWorksOnEdit()
            {
                var ids = ITH.CreateUsers(app, 2).ToList();
                using(var id1 = ids[0])
                using(var id2 = ids[1])
                {
                    app.NavigateTo<AccountController>(c => c.Edit(id2));
                    var form = app.FindFormFor<UserCreateModel>()
                        .Field(f => f.Username).SetValueTo(id1.Username)
                        .Field(f => f.Name).Click(); // Set focus


                    var validation = app.WaitForElementToBeVisible(By.CssSelector("input#Username~span.field-validation-error>span"), TimeSpan.FromSeconds(1), true);
                    Assert.AreEqual(Resources.Validation_Duplicate_Name, validation.Text);

                    var input = app.Browser.FindElementByCssSelector("input#Username");
                    Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));
                }
                ids.Clear();
            }

            [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
            public void UsernameEnsureDuplicationDetectionStillAllowsEditOtherProperties()
            {
                var ids = ITH.CreateUsers(app, 1).ToList();
                using(var id1 = ids[0])
                {
                    app.NavigateTo<AccountController>(c => c.Edit(id1));
                    var field = app.FindFormFor<UserCreateModel>() 
                        .Field(f => f.Name);
                    field.ValueShouldEqual("Name" + id1.Username.Substring(id1.Username.Length - 1));
                    field.SetValueTo("somename")
                        .Submit();

                    app.NavigateTo<AccountController>(c => c.Edit(id1)); // force refresh
                    app.FindFormFor<UserCreateModel>()
                        .Field(f => f.Name).ValueShouldEqual("somename");
                }
                ids.Clear();
            }
        }
    }
}
