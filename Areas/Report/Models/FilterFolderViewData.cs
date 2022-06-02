namespace Cyara.Web.Portal.Areas.Report.Models
{
    using System.Collections.Generic;

    using Cyara.Domain.Types.Shared;
    using Cyara.Shared.Types.Storage;

    public class FilterFolderViewData
    {
        public string Title { get; set; }

        public bool IsLazy { get; set; }

        public string Key { get; set; }

        public bool IsFolder { get; set; }

        public bool? HideCheckbox { get; set; }

        public bool? Select { get; set; }

        public bool? Unselectable { get; set; }

        public string AddClass { get; set; }

        public EntityType EntityType { get; set; }

        public FolderType FolderType { get; set; }

        public IEnumerable<FilterFolderViewData> Children { get; set; }
    }
}