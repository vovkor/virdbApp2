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
    
    public partial class storage
    {
        public storage()
        {
            this.maindb = new HashSet<maindb>();
        }
    
        public int code { get; set; }
        public string fullname { get; set; }
    
        public virtual ICollection<maindb> maindb { get; set; }
    }
}