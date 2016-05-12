using appProveedores.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(appProveedores.Startup))]
namespace appProveedores
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            createRolesandUsers();

        }
        private void createRolesandUsers()
        {
            ApplicationDbContext context = new ApplicationDbContext();

            var rolManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            if (!rolManager.RoleExists("Administrador"))
            {
                var role = new IdentityRole();
                role.Name = "Administrador";
                rolManager.Create(role);

                var user = new ApplicationUser();
                user.UserName = "Administrador@gmail.com";
                user.Email = "Administrador@gmail.com";

                string userPWD = "Admin123";
                var chkuser = userManager.Create(user, userPWD);
                if (chkuser.Succeeded)
                {
                    var reulst1 = userManager.AddToRole(user.Id, "Administrador");

                }
            }

            if (!rolManager.RoleExists("Cliente"))
            {
                var role = new IdentityRole();
                role.Name = "Cliente";
                rolManager.Create(role);
            }
        }
    }
}
