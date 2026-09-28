using Microsoft.AspNetCore.Mvc.Rendering;

namespace NewsWebApp.Models.ViewModels
{
    public class ArticlesVM
    {
        public List<Articles> Articles { get; set; } = new List<Articles>();

       

        public string status { get; set; }

        public List<SelectListItem> Statuses = new List<SelectListItem>
        {
            new SelectListItem { Value = "New", Text = "New" },
            new SelectListItem { Value = "Pending", Text = "Pending" },
            new SelectListItem { Value = "Approved", Text = "Approved" },
            new SelectListItem { Value = "Rejected", Text = "Rejected" },
            new SelectListItem { Value = "Archived", Text = "Archived" }
        };


    }
}
