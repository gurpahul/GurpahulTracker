﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GurBhugTracker.Models
{
    public class TicketComment
    {
        public int Id { get; set; }
        public string Cpmment { get; set; }
        public DateTimeOffset Created { get; set; }
        public int TicketId { get; set; }
        public virtual Ticket Ticket { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

    }
}