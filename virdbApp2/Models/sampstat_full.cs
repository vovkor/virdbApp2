//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace virdbApp2.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class sampstat_full
    {
        public sampstat_full()
        {
            this.maindb = new HashSet<maindb>();
        }
    
        public int sampstat { get; set; }
        public string fullname { get; set; }
        public string stat_code { get; set; }
        public string name_eng { get; set; }
    
        public virtual ICollection<maindb> maindb { get; set; }
    }
}
