using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoreManager.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace CoreApi.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }
        public ActionResult TestApi()
        {
            ViewBag.Title = "test";

            return View();
        }
        public ActionResult RegisterNewRole()
        {
            var roleManager = new RoleManager<ApplicationRole,int>(new ApplicationRoleStore(new ApplicationDbContext()));
            // Create Admin Role
            //string roleName = UserRoles.TaxiAgencyAdmin.ToString();
            string roleName = UserRoles.WebUser.ToString();
            IdentityResult roleResult;

            // Check to see if Role Exists, if not create it
            if (!roleManager.RoleExists(roleName))
            {
                roleResult = roleManager.Create<ApplicationRole,int>(new ApplicationRole(roleName));
            }
            return null;
        }
    }
}
