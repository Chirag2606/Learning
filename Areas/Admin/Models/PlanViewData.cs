namespace Cyara.Web.Portal.Areas.Admin.Models
{
    public class PlanViewData
    {
        public int PlanId { get; set; }

        public string Name { get; set; }

        public string MediaType { get; set; }

        public string Type { get; set; }

        public string Start { get; set; }

        public string End { get; set; }

        public bool Active { get; set; }
    }
}