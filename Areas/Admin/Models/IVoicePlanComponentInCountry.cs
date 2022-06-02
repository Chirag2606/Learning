namespace Cyara.Web.Portal.Areas.Admin.Models
{
    public interface IVoicePlanComponentInCountry
    {
        bool InCountryLicensed { get; set; }

        bool InCountryEnabled { get; set; }
    }
}