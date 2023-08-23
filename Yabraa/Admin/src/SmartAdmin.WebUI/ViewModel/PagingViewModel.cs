using System.Collections.Generic;

namespace SmartAdmin.WebUI.ViewModel
{
    public class PagingViewModel<T>
    {

        public List<T> items { get; set; }
        public int CurrentPageIndex { get; set; }
        public int PageCount { get; set; }
        public int itemsCount { get; set; }
        public int Tablelength { get; set; }

    }
}
