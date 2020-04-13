using System;
using System.Collections.Generic;

namespace NetworkTreeWebApp.Data
{
    public partial class Account
    {
        public Account()
        {
            Children = new HashSet<Account>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public int PlacementPreference { get; set; }
        public int Leg { get; set; }
        public long? ParentId { get; set; }

        public virtual Account Parent { get; set; }
        public virtual ICollection<Account> Children { get; set; }
    }
}
