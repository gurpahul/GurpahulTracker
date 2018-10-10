using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GurBhugTracker.Models
{
    public class Projects
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public virtual ICollection<ApplicationUser> User { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }

        public Projects()
        {
            User = new HashSet<ApplicationUser>();
            Tickets = new HashSet<Ticket>();
        }
    }
}