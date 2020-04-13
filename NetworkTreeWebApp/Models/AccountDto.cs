using System;
using System.Collections.Generic;

namespace NetworkTreeWebApp.Models
{
    public class AccountDto
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public int PlacementPreference { get; set; }
        public int Leg { get; set; }
        public long? ParentId { get; set; }

        public bool Selectable { get; set; }
        public List<AccountDto> Nodes { get; set; }
    }
}
