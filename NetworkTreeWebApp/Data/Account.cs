using System;
using System.Collections.Generic;

namespace NetworkTreeWebApp.Data
{
    public partial class Account
    {
        public Account()
        {
            Children = new List<Account>();
            Downlinks = new List<Account>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public int PlacementPreference { get; set; }
        public int Leg { get; set; }
        public long? ParentId { get; set; }
        public long? UplinkId { get; set; }

        public  List<Account> Children { get; set; }
        public List<Account> Downlinks { get; set; }
    }
}
