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
    }
}
