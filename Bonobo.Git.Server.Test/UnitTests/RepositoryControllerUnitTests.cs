using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Data.Update;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Test.MembershipTests.EFTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpecsFor.Mvc.Helpers;

namespace Bonobo.Git.Server.Test.UnitTests
{
    [TestClass]
    public class RepositoryControllerUnitTests
    {
        private SqliteTestConnection _testDb;
        private RepositoryController _controller;

        [TestInitialize]
        public void Initialise()
        {
            _testDb = new SqliteTestConnection();
            new AutomaticUpdater().RunWithContext(_testDb.GetContext());
            TestHelpers.InitialiseGlobalConfig();
            UserConfiguration.Current.RepositoryPath = Path.GetTempPath();
            _controller = new RepositoryController();
            _controller.RepositoryRepository = new EFRepositoryRepository {CreateContext = () => _testDb.GetContext()};
            _controller.RepositoryPermissionService = new PermissiveService();
        }

        private Guid CreateRepo()
        {
            var repository = new RepositoryModel()
            {
                Name = "Test"
            };
            _controller.RepositoryRepository.Create(repository);
            return repository.Id;
        }

        [TestMethod]
        public void DetailModelGeneralUrlCorrectlyGenerated()
        {
            var repoId = CreateRepo();
            RequestContext request = new RequestContext();
            _controller.ControllerContext = new ControllerContext (request, _controller);
            _controller.Request;

            HttpRequestBase
            var view = _controller.Detail(repoId);  
        }
         
    }
}