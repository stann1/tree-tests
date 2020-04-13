using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NetworkTreeWebApp.Data;
using NetworkTreeWebApp.Models;

namespace NetworkTreeWebApp.Utils
{
    public class TreeBuilder
    {
        public static AccountDto BuildTree(List<Account> accounts)
        {
            var dtos = accounts.Select(a => new AccountDto(){
                Id = a.Id,
                Text = a.Name,
                PlacementPreference = a.PlacementPreference,
                Leg = a.Leg,
                ParentId = a.ParentId,
                Selectable = false,
                Nodes = new List<AccountDto>() 
            }).ToList();

            int counter = 0;
            Stopwatch sw = Stopwatch.StartNew();
            
            var root = dtos.FirstOrDefault();
            var childNode = GetChildNodes(root, dtos.Where(n => n.Id != root.Id).ToList(), new Dictionary<long, bool>(), ref counter);
            
            sw.Stop();
            System.Console.WriteLine($"Total iterations: {counter}. Took {sw.Elapsed} ms");

            root.Nodes = childNode;
            return root;
        }

        private static List<AccountDto> GetChildNodes(AccountDto node, List<AccountDto> allNodes, Dictionary<long, bool> foundNodes, ref int counter)
        {
            counter++;
            foundNodes[node.Id] = true;
            if(counter % 1000 == 0)
            {
                System.Console.WriteLine("Processed recursive calls: " + counter);
            }

            var children = new List<AccountDto>();

            //var availableNodes = allNodes.Where(n => !foundNodes.ContainsKey(n.Id)).ToList();

            foreach (var item in allNodes.Where(n => n.ParentId == node.Id))
            {
                if(!foundNodes.ContainsKey(item.Id))
                {
                    item.Nodes = GetChildNodes(item, allNodes, foundNodes, ref counter);
                }
                children.Add(item);
            }

            return children;
        }
    }
}
