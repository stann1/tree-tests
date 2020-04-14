using System;
using System.Collections.Generic;
using Microsoft.SqlServer.Types;

namespace NetworkTreeWebApp.Data
{
    public partial class AccountHierarchy
    {
        public AccountHierarchy()
        {
            Children = new List<AccountHierarchy>();
            Downlinks = new List<AccountHierarchy>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public int PlacementPreference { get; set; }
        public int Leg { get; set; }
        public long? ParentId { get; set; }
        public long? UplinkId { get; set; }

        public SqlHierarchyId Level { get; set; }

        public  List<AccountHierarchy> Children { get; set; }
        public List<AccountHierarchy> Downlinks { get; set; }
    }
}
