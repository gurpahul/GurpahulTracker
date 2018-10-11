using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace GurBhugTracker.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public ICollection<Projects> Projects { get; set; }
        public string DisplayName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Name { get; set; }

        [InverseProperty("Creater")]
        public virtual ICollection<Ticket> CreatedTickets { get; set; }
        [InverseProperty("Assignee")]
        public virtual ICollection<Ticket> AssignedTickets { get; set; }


        public ApplicationUser()
        {
            Projects = new HashSet<Projects>();
        }



        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<GurBhugTracker.Models.Projects> Projects { get; set; }



        public System.Data.Entity.DbSet<GurBhugTracker.Models.Ticket> Tickets { get; set; }

        public System.Data.Entity.DbSet<GurBhugTracker.Models.TicketStatus> TicketStatuses { get; set; }
        public System.Data.Entity.DbSet<GurBhugTracker.Models.TicketPriority> TicketPriorities { get; set; }
        public System.Data.Entity.DbSet<GurBhugTracker.Models.TicketType> TicketTypes { get; set; }


    }

}