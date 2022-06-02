namespace Cyara.Web.Portal.Areas.Admin.Models
{
    public class SettingViewData
    {
        public string SettingKey { get; set; }

        public string SettingCode { get; set; }

        public string SettingValue { get; set; }

        public string SettingLabel { get; set; }

        public string SettingTooltip { get; set; }

        public bool ReadOnly { get; set; }
    }
}