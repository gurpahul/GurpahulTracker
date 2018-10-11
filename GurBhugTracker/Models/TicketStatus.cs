﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GurBhugTracker.Models
{
    public class TicketStatus
    {
        public int Id { get; set; }
        public String Name { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}