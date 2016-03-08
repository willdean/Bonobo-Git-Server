using Bonobo.Git.Server.Attributes;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace Bonobo.Git.Server.Controllers
{
    [OutputCache(Location = OutputCacheLocation.None, NoStore = true)]
    public class ValidationController : Controller
    {
        [Dependency]
        public IRepositoryRepository RepoRepo { get; set; }

        [Dependency]
        public IMembershipService MembershipService { get; set; }

        [Dependency]
        public ITeamRepository TeamRepo { get; set; }

        public ActionResult UniqueNameRepo(string name, string guid)
        {
            //Guid id = guid.HasValue ? guid.Value : Guid.Empty;
            string sid = Request.QueryString["Id"];
            Guid id = sid == "undefined" ? Guid.Empty : Guid.Parse(sid);
            var existing_repo = new RepositoryDetailModel();
            try
            {
                existing_repo = RepositoryController.ConvertRepositoryModel(RepoRepo.GetRepository(id), User);
            }catch(Exception)
            {
            }
            var validationContext = new ValidationContext(existing_repo);
            // This will do two repository lookups at least but keeps the
            // logic behind the validation the same.
            UniqueRepoNameAttribute uniqRepoName = new UniqueRepoNameAttribute();
            var result = uniqRepoName.GetValidationResult(name, validationContext);
            return Json(result == System.ComponentModel.DataAnnotations.ValidationResult.Success, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UniqueNameUser(string Username, Guid? guid)
        {
            //Guid id = guid.HasValue ? guid.Value : Guid.Empty;
            string sid = Request.QueryString["Id"];
            Guid id = sid == "undefined" ? Guid.Empty : Guid.Parse(sid);
            var possibly_existent_user = MembershipService.GetUserModel(Username);
            bool exists = (possibly_existent_user != null) && (id != possibly_existent_user.Id);
            return Json(!exists, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UniqueNameTeam(string name, Guid? guid)
        {
            //Guid id = guid.HasValue ? guid.Value : Guid.Empty;
            string sid = Request.QueryString["Id"];
            Guid id = sid == "undefined" ? Guid.Empty : Guid.Parse(sid);
            var possibly_existing_team = TeamRepo.GetTeam(name);
            bool exists = (possibly_existing_team != null) && (id != possibly_existing_team.Id);
            // false when repo exists!
            return Json(!exists, JsonRequestBehavior.AllowGet);
        }
    }
}
