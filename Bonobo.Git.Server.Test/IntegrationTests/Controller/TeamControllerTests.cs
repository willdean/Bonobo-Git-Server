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
    public class HomeControllerSpecs
    {
        [TestClass]
        public class TeamControllerTests
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
            public void TeamNameEnsureDuplicationDetectionAsYouTypeWorksOnCreation()
            {
                using (var id1 = ITH.CreateTeams(app, 1).Single())
                {
                    app.NavigateTo<TeamController>(c => c.Create());
                    var form = app.FindFormFor<TeamEditModel>()
                        .Field(f => f.Name).SetValueTo(id1.Name)
                        .Field(f => f.Description).Click(); // Set focus


                    var input = app.Browser.FindElementByCssSelector("input#Name");
                    Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));
                }
            }

            [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
            public void TeamNameEnsureDuplicationDetectionAsYouTypeWorksOnEdit()
            {
                var ids = ITH.CreateTeams(app, 2).ToList();
                using (var id1 = ids[0])
                using (var id2 = ids[1])
                {
                    app.NavigateTo<TeamController>(c => c.Edit(id2));
                    var form = app.FindFormFor<TeamEditModel>()
                        .Field(f => f.Name).SetValueTo(id1.Name)
                        .Field(f => f.Description).Click(); // Set focus


                    var validation = app.WaitForElementToBeVisible(By.CssSelector("input#Name~span.field-validation-error>span"), TimeSpan.FromSeconds(1), true);
                    Assert.AreEqual(Resources.Validation_Duplicate_Name, validation.Text);

                    var input = app.Browser.FindElementByCssSelector("input#Name");
                    Assert.IsTrue(input.GetAttribute("class").Contains("input-validation-error"));

                }
                ids.Clear();
            }

            [TestMethod, TestCategory(TestCategories.WebIntegrationTest)]
            public void TeamNameEnsureDuplicationDetectionStillAllowsEditOtherProperties()
            {
                var ids = ITH.CreateTeams(app, 1).ToList();
                using(var id1 = ids[0])
                {
                    app.NavigateTo<TeamController>(c => c.Edit(id1));
                    var field = app.FindFormFor<TeamEditModel>()
                        .Field(f => f.Description);
                    field.ValueShouldEqual("Nice team number " + id1.Name.Substring(id1.Name.Length - 1));
                    field.SetValueTo("somename")
                        .Submit();

                    app.NavigateTo<TeamController>(c => c.Edit(id1)); // force refresh
                    app.FindFormFor<TeamEditModel>()
                        .Field(f => f.Description).ValueShouldEqual("somename");
                }
                ids.Clear();
            }
        }
    }
}
