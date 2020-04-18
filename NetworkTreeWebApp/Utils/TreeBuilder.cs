using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Types;
using NetworkTreeWebApp.Data;
using NetworkTreeWebApp.Models;

namespace NetworkTreeWebApp.Utils
{
    public class TreeBuilder
    {
        private readonly AccountsContext _dbContext;

        public TreeBuilder(AccountsContext dbContext)
        {
            this._dbContext = dbContext;
        }
        public AccountDto BuildTreeInMemory(Account rootNode)
        {
            var dtos = _dbContext.Accounts.OrderBy(a => a.Id).Where(a => a.Id > rootNode.Id).Select(AccountDto.MapToFromEntity).ToList();
            System.Console.WriteLine($"Building for {rootNode.Name}, with {dtos.Count} entities");

            int counter = 0;
            Stopwatch sw = Stopwatch.StartNew();
            
            var root = AccountDto.MapToFromEntity(rootNode);
            var childNodes = GetChildNodes(root, dtos, new Dictionary<long, bool>(), ref counter);
            
            sw.Stop();
            System.Console.WriteLine($"Memory - Total iterations: {counter}. Took {sw.ElapsedMilliseconds} ms");

            root.Nodes = childNodes;
            return root;
        }

        private List<AccountDto> GetChildNodes(AccountDto node, List<AccountDto> allNodes, Dictionary<long, bool> foundNodes, ref int counter)
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

        public async Task<AccountDto> BuildTreeInDbRecursive(long id)
        {
            var root = await _dbContext.Accounts.Include(a => a.Children).FirstOrDefaultAsync(a => a.Id == id);
            // var children = GetChildNodesRecursive(root, _dbContext.Accounts.Where(a => a.ParentId == root.Id));
            Stopwatch sw = Stopwatch.StartNew();
            RecursiveLoad(root);
            System.Console.WriteLine($"SQL - Recursive load finished in {sw.ElapsedMilliseconds} ms");

            sw.Restart();
            var result = MapToDtoRecursive(root);
            System.Console.WriteLine($"SQL - Recursive mapping finished in {sw.ElapsedMilliseconds} ms");

            return result;
        }

        private void RecursiveLoad(Account parent)
        {
            if (parent == null) {
                return;
            } else {
                _dbContext.Entry(parent).Collection(m => m.Children).Load();
                foreach (var entry in parent.Children) {
                    RecursiveLoad(entry);
                }
            }
        }

        private AccountDto MapToDtoRecursive(Account source)
        {
            var target = AccountDto.MapToFromEntity(source);

            foreach (var item in source.Children)
            {
                var mappedChild = MapToDtoRecursive(item);
                target.Nodes.Add(mappedChild);
            }

            return target;
        }

        public AccountDto BuildHierarchyTreeRecursive(AccountHierarchy rootNode)
        {
            var dtos = _dbContext.AccountHierarchies
                .OrderBy(a => a.LevelPath.Length).ThenBy(a => a.LevelPath)
                .Where(a => a.LevelPath.StartsWith(rootNode.LevelPath))
                //.Take(10000)   // temp for testing
                .Select(AccountDto.MapToFromHierarchyEntity).ToList();
            
            System.Console.WriteLine($"Building for {rootNode.Name}, with {dtos.Count} entities");
            
            var bottom = dtos.Last();
            System.Console.WriteLine($"Bottom is {bottom.Id} with level {bottom.Level}, hierarchy depth {bottom.Depth}");

            int counter = 0;
            Stopwatch sw = Stopwatch.StartNew();

            var levels = bottom.Level.Split("/", StringSplitOptions.RemoveEmptyEntries);
            // System.Console.WriteLine(levels.Length);

            AccountDto root = AccountDto.MapToFromHierarchyEntity(rootNode);
            root.Nodes = SetChildNodesOnDepthRecursive(root.Depth + 1, bottom.Depth, root, dtos, ref counter);

            sw.Stop();
            System.Console.WriteLine($"Hierarchy loop - Total iterations: {counter}. Took {sw.ElapsedMilliseconds} ms");

            return root;
        }

        private List<AccountDto> SetChildNodesOnDepthRecursive(int depth, int maxDepth, AccountDto node, List<AccountDto> allNodes, ref int counter)
        {
            counter++;
            if(counter % 1000 == 0)
            {
                System.Console.WriteLine("Processed recursive calls: " + counter);
            }
            
            var children = new List<AccountDto>();
            if(depth > maxDepth)
            {
                return children;
            }


            foreach (var item in allNodes.Where(n => n.Depth == depth))
            {
                if(item.ParentId == node.Id)
                {
                    item.Nodes = SetChildNodesOnDepthRecursive(depth + 1, maxDepth, item, allNodes, ref counter);
                    children.Add(item);
                }
            }

            return children;
        }

        public void CalculateBonuses(AccountHierarchy rootNode)
        {
            // var entities = new List<AccountHierarchy>()
            // {
            //     new AccountHierarchy(){ Id = 1, Name = "A", PlacementPreference = 3, ParentId = null, UplinkId = null, LevelPath = "/" },
            //     new AccountHierarchy(){ Id = 2, Name = "B", PlacementPreference = 2, ParentId = 1, UplinkId = 1, LevelPath = "/1/" },
            //     new AccountHierarchy(){ Id = 3, Name = "C", PlacementPreference = 3, ParentId = 1, UplinkId = 1, LevelPath = "/2/" },
            //     new AccountHierarchy(){ Id = 4, Name = "D", PlacementPreference = 3, ParentId = 2, UplinkId = 1, LevelPath = "/1/1/" },
            //     new AccountHierarchy(){ Id = 5, Name = "H", PlacementPreference = 1, ParentId = 3, UplinkId = 1, LevelPath = "/2/1/" },
            //     new AccountHierarchy(){ Id = 6, Name = "K", PlacementPreference = 3, ParentId = 3, UplinkId = 1, LevelPath = "/2/2/" },
            //     new AccountHierarchy(){ Id = 7, Name = "F", PlacementPreference = 2, ParentId = 2, UplinkId = 1, LevelPath = "/1/2/" },
            //     new AccountHierarchy(){ Id = 8, Name = "G", PlacementPreference = 3, ParentId = 4, UplinkId = 1, LevelPath = "/1/1/1/" },
            //     new AccountHierarchy(){ Id = 9, Name = "V", PlacementPreference = 3, ParentId = 4, UplinkId = 1, LevelPath = "/1/1/2/" },
            //     new AccountHierarchy(){ Id = 10, Name = "L", PlacementPreference = 1, ParentId = 6, UplinkId = 1, LevelPath = "/2/2/1/" },
            //     new AccountHierarchy(){ Id = 11, Name = "O", PlacementPreference = 3, ParentId = 10, UplinkId = 1, LevelPath = "/2/2/1/1/" }
            // };
            Stopwatch sw = Stopwatch.StartNew();
            var entities = _dbContext.AccountHierarchies
                .OrderBy(a => a.LevelPath.Length).ThenBy(a => a.LevelPath)
                .Where(a => a.LevelPath.StartsWith(rootNode.LevelPath));

            var dtos = entities.Select(AccountDto.MapToFromHierarchyEntity).ToList();
            System.Console.WriteLine($"Start processing {dtos.Count} items.");

            Dictionary<string,long> pathBonus = new Dictionary<string, long>();
            for (int i = dtos.Count - 1; i >= 0; i--)
            {
                var entity = dtos[i];
                long bonus = entity.Id;
                if(pathBonus.ContainsKey(entity.Level))
                {
                    bonus += pathBonus[entity.Level];
                }

                var parentPathNode = SqlHierarchyId.Parse(entity.Level).GetAncestor(1);
                string parentPath = parentPathNode.ToString();

                if(!pathBonus.ContainsKey(parentPath))
                {
                    pathBonus[parentPath] = 0;
                }

                pathBonus[parentPath] += bonus;

                if (i % 1000 == 0)
                {
                    System.Console.WriteLine($"Processed next 1000 of {i} remaining");
                }

                //System.Console.WriteLine($"Bonus for {entity.Text}: {bonus}");
            }

            System.Console.WriteLine($"Finished in {sw.ElapsedMilliseconds} ms");
        }
    }
}
