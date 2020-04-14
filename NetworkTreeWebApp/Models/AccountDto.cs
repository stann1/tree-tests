using System;
using System.Collections.Generic;
using NetworkTreeWebApp.Data;

namespace NetworkTreeWebApp.Models
{
    public class AccountDto
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public int PlacementPreference { get; set; }
        public int Leg { get; set; }
        public long? ParentId { get; set; }
        public long? UplinkId { get; set; }

        public bool Selectable { get; set; }
        public List<AccountDto> Nodes { get; set; }

        public static AccountDto MapToFromEntity(Account acc)
        {
            return new AccountDto(){
                Id = acc.Id,
                Text = acc.Name,
                PlacementPreference = acc.PlacementPreference,
                Leg = acc.Leg,
                ParentId = acc.ParentId,
                UplinkId = acc.UplinkId,
                Selectable = false,
                Nodes = new List<AccountDto>() 
            };
        }
    }
}
