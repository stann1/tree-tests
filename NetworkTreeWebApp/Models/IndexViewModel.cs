using System;
using System.Collections.Generic;
using NetworkTreeWebApp.Data;

namespace NetworkTreeWebApp.Models
{
    public class IndexViewModel
    {
        public List<Account> Accounts { get; set; }
        public List<AccountHierarchy> AccountHierarchies { get; set; }
        public int CountRegular { get; set; }
        public int CountHierarchies { get; set; }
    }
}
