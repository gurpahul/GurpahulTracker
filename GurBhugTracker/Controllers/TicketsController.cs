using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using GurBhugTracker.Models;
using Microsoft.AspNet.Identity;

namespace GurBhugTracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserRoleHelper URH = new UserRoleHelper();
             
        // GET: Tickets 
        public ActionResult Index()
        {
            var tickets = db.Tickets.Include(t => t.Assignee).Include(t => t.Creater).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            return View(tickets.ToList());
        }


        //: Assign Users



        // Get Submitter Tickets
        public ActionResult SubmitterTickets()
        {
            var user = User.Identity.GetUserId();
            if (User.IsInRole("Submitter"))
            {
                var tickets = db.Tickets.Where(t => t.CreaterId == user).Include(t => t.Creater).Include(t => t.Assignee).Include(t => t.Project);
                return View("Index", tickets.ToList());
            }
            if (User.IsInRole("Developer"))
            {
                var tickets = db.Tickets.Where(t => t.AssigneeId == user).Include(t => t.Comments).Include(t => t.Creater).Include(t => t.Assignee).Include(t => t.Project);
                return View("Index", tickets.ToList());
            }
            if (User.IsInRole("Project Manager"))
            {
                return View(db.Tickets.Include(t => t.TicketPriority).Include(t => t.Project).Include(t => t.Comments).Include(t => t.TicketStatus).Include(t => t.TicketType).Where(p => p.AssigneeId == user).ToList());
            }
            return View("Index");

        }

        public  ActionResult AssignDevelopers(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }

            var model = new AssinDevlViewModal();


            var tiket = db.Tickets.FirstOrDefault(x => x.Id == id);

            
            model.DeveloperId = tiket.AssigneeId;
            


            var developerrole = db.Roles.Where(x => x.Name == "Developer").FirstOrDefault().Id;
            var users = db.Users.Where(x => x.Roles.Any(c => c.RoleId == developerrole)).ToList();
        


            model.user = new MultiSelectList(users, "Id", "Name", null, "Id");

            return View(model);
        }

        [HttpPost]
        public ActionResult AssignDevelopers(AssinDevlViewModal model)
        {
            //STEP 1: Find the ticket.

            var tiket = db.Tickets.FirstOrDefault(x => x.Id == model.Id);
            //STEP 2: Assign the developer that the user select to the ticket.

            tiket.AssigneeId = model.DeveloperId;
            var user = db.Users.FirstOrDefault(i => i.Id == model.DeveloperId);
            //STEP 3: Save the ticket.
            var personalEmailService = new PersonalEmailService();
            var mailMessage = new MailMessage(
               WebConfigurationManager.AppSettings["emailto"],
              user
               .Email);
            mailMessage.Body = "hi";
            mailMessage.Subject = "hi";
            mailMessage.IsBodyHtml = true;
            personalEmailService.Send(mailMessage);
            db.SaveChanges();
            return RedirectToAction("Index");
          
        }


        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        [HttpPost]
        public ActionResult CreateComment(int id, string body)
        {
            var tickets = db.Tickets
               .Where(p => p.Id == id)
               .FirstOrDefault();
            var useR = User.Identity.GetUserId();
            if (tickets == null)
            {
                return HttpNotFound();
            }
            if (string.IsNullOrWhiteSpace(body))
            {
                ViewBag.ErrorMessage = "Comment is required";
                return View("Details", tickets);
            }
            if ((User.IsInRole("Submitter") && tickets.CreaterId == useR
                ) || (User.IsInRole("Developer") && tickets.AssigneeId == useR
                ) || (User.IsInRole("Project Manager") && tickets.Project.User.Select(p => p.Id).Contains(useR)))
            {
                var comment1 = new TicketComment();
                comment1.UserId = User.Identity.GetUserId();
                comment1.TicketId = tickets.Id;
                comment1.Created = DateTime.Now;
                comment1.Cpmment = body;
                var userid = db.Users.FirstOrDefault(c => c.Id == comment1.UserId);

                db.TicketComments.Add(comment1);

                // Plug in your email service here to send an email.
                var personalEmailService = new PersonalEmailService();
                var mailMessage = new MailMessage(
                   WebConfigurationManager.AppSettings["emailto"],
                   userid.Email);
                mailMessage.Body = "hi";
                mailMessage.Subject = "hi";
                mailMessage.IsBodyHtml = true;
                personalEmailService.Send(mailMessage);
                db.SaveChanges();

            }
            return RedirectToAction("Details", new { id });
        }

        // GET: Tickets/Create
        [Authorize(Roles = "Submitter")]
        public ActionResult Create()
        {
            ViewBag.AssigneeId = new SelectList(db.Users, "Id", "DisplayName");
            ViewBag.CreaterId = new SelectList(db.Users, "Id", "DisplayName");
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Submitter")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,TicketTypeId,TicketPriorityId,ProjectId")] Ticket ticket)
        {

            if (ModelState.IsValid)
            {
                if (ticket == null)
                {
                    return HttpNotFound();
                }
                ticket.TicketStatusId = 3;
                db.Tickets.Add(ticket);

                db.SaveChanges();
                return RedirectToAction("Index");
            }


            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Submitter")]
        public ActionResult CreateAttachment(int ticketId, [Bind(Include = "Id,Description,TicketTypeId")] TicketAttachment ticketAttachment, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                var tickets = db.Tickets.FirstOrDefault(t => t.Id == ticketId);
                var userId = User.Identity.GetUserId();
                var user = db.Users.FirstOrDefault(i => i.Id == userId);
                if (!ImageUploadValidator.IsWebFriendlyImage(image))
                {
                    ViewBag.ErrorMessage = "Please upload an image";

                }
                if (image == null)
                {
                    return HttpNotFound();
                }
                if ((User.IsInRole("Submitter") && tickets.CreaterId == userId) || (User.IsInRole("Developer") && tickets.AssigneeId == userId) || (User.IsInRole("Project Manager") && tickets.Project.User.Select(p => p.Id).Contains(userId)))
                {

                    var fileName = Path.GetFileName(image.FileName);
                    image.SaveAs(Path.Combine(Server.MapPath("~/Uploads/"), fileName));
                    ticketAttachment.FilePath = "/Uploads/" + fileName;
                    ticketAttachment.UserId = User.Identity.GetUserId();
                    ticketAttachment.Created = DateTime.Now;
                    ticketAttachment.UserId = User.Identity.GetUserId();
                    ticketAttachment.TicketId = ticketId;
                    db.TicketAttachments.Add(ticketAttachment);
                    var personalEmailService = new PersonalEmailService();
                    var mailMessage = new MailMessage(
                       WebConfigurationManager.AppSettings["emailto"],
                       user.Email);
                    mailMessage.Body = "hi";
                    mailMessage.Subject = "hi";
                    mailMessage.IsBodyHtml = true;
                    personalEmailService.Send(mailMessage);
                    db.SaveChanges();
                }
                    return RedirectToAction("Details", new { id = ticketId });
                
            }
            return View(ticketAttachment);
        }


        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            ViewBag.AssigneeId = new SelectList(db.Users, "Id", "DisplayName", ticket.AssigneeId);
            ViewBag.CreaterId = new SelectList(db.Users, "Id", "DisplayName", ticket.CreaterId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,TicketTypeId,TicketPriorityId,ProjectId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                var dateChanged = DateTimeOffset.Now;
                var changes = new List<TicketHistory>();
                var dbTicket = db.Tickets.First(p => p.Id == ticket.Id);
                dbTicket.Name = ticket.Name;
                dbTicket.Description = ticket.Description;
                dbTicket.TicketTypeId = ticket.TicketTypeId;
                dbTicket.Updated = dateChanged;
                var originalValues = db.Entry(dbTicket).OriginalValues;
                var currentValues = db.Entry(dbTicket).CurrentValues;
                foreach (var property in originalValues.PropertyNames)
                {
                    var originalValue = originalValues[property]?.ToString();
                    var currentValue = currentValues[property]?.ToString();
                    if (originalValue != currentValue)
                    {
                        var history = new TicketHistory();
                        history.OldValue = originalValue;
                        history.Changed = dateChanged;
                        history.Property = property;
                        history.NewValue = currentValue;
                        history.TicketId = dbTicket.Id;
                        history.UserId = User.Identity.GetUserId();
                        changes.Add(history);
                    }
                }
                db.TicketHistories.AddRange(changes);


                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);

            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }
            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            db.Tickets.Remove(ticket);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

}
