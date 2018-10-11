﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GurBhugTracker.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }

        public int TicketTypeId { get; set; }
        public virtual TicketType TicketType { get; set; }
        public int TicketPriorityId { get; set; }
        public virtual TicketPriority TicketPriority { get; set; }
        public string CreaterId { get; set; }
        public virtual ApplicationUser Creater { get; set; }
        public int TicketStatusId { get; set; }
        public virtual TicketStatus TicketStatus { get; set; }
        public string AssigneeId { get; set; }
        public virtual ApplicationUser Assignee { get; set; }

        public int ProjectId { get; set; }
        public virtual Projects Project { get; set; }

        public Ticket()
        {
            this.Created = DateTime.Now;
        }

    }
}