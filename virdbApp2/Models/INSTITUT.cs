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
    
    public partial class INSTITUT
    {
        public INSTITUT()
        {
            this.maindb = new HashSet<maindb>();
            this.maindb1 = new HashSet<maindb>();
            this.maindb2 = new HashSet<maindb>();
        }
    
        public string NA { get; set; }
        public string NAMEINST { get; set; }
        public string NAMEENG { get; set; }
        public string FAO { get; set; }
        public string Дубль { get; set; }
        public Nullable<double> GEOCOD { get; set; }
        public string GEOENGNEW { get; set; }
        public string ADRES { get; set; }
        public string EMAIL { get; set; }
        public string TEL { get; set; }
        public string FAX { get; set; }
        public string KATINST { get; set; }
        public string ACRONYM { get; set; }
        public string NOTES { get; set; }
    
        public virtual ICollection<maindb> maindb { get; set; }
        public virtual ICollection<maindb> maindb1 { get; set; }
        public virtual ICollection<maindb> maindb2 { get; set; }
    }
}
