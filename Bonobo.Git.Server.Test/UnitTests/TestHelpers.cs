using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;

namespace Bonobo.Git.Server.Test.UnitTests
{
    public class TestHelpers
    {
        public static void InitialiseGlobalConfig()
        {
// This file should never actually get created, but ConfigurationManager needs it for its static initialisation
            var configFileName = Path.Combine(Path.GetTempFileName(), "BonoboTestConfig.xml");
            ConfigurationManager.AppSettings["UserConfiguration"] = configFileName;
            UserConfiguration.InitialiseForTest();
        }
    }

    /// <summary>
    /// Very permissive repository permission service - permits everything
    /// </summary>
    class PermissiveService : IRepositoryPermissionService
    {
        public bool HasPermission(Guid userId, Guid repositoryId, RepositoryAccessLevel requiredLevel)
        {
            return true;
        }

        public bool HasCreatePermission(Guid userId)
        {
            return true;
        }

        public IEnumerable<RepositoryModel> GetAllPermittedRepositories(Guid userId, RepositoryAccessLevel requiredLevel)
        {
            throw new System.NotImplementedException();
        }

        public bool HasPermission(Guid userId, string repositoryName, RepositoryAccessLevel requiredLevel)
        {
            return true;
        }
    }
}