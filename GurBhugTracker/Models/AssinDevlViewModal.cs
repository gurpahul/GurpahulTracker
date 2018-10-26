using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GurBhugTracker.Models
{
    public class AssinDevlViewModal
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public SelectList  Majhbhi {get; set;}
        public MultiSelectList user { get; set; }
        public string DeveloperId { get; set; }
    }
}