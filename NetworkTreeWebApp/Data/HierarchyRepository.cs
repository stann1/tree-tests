using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Types;
using NetworkTreeWebApp.Models;

namespace NetworkTreeWebApp.Data
{
    public class HierarchyRepository
    {
        private readonly AccountsContext _dbContext;

        public HierarchyRepository(AccountsContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<List<AccountHierarchy>> GetByLevelRaw(int level)
        {
            string query = $@"SELECT 
                [Id],
                [Name],
                [PlacementPreference],
                [Leg],
                [ParentId], 
                [UplinkId],
                [Level].ToString() AS Level
                     FROM AccountHierarchy WHERE [Level].GetLevel() = {level}";
            var topLevelQuery = _dbContext.AccountHierarchies.FromSqlRaw(query);
            return await topLevelQuery.ToListAsync();
        }

        public async Task<List<AccountHierarchy>> GetByLevel(int level)
        {
            var topLevelQuery = _dbContext.AccountHierarchies;
            return await topLevelQuery.ToListAsync();
        }

        public async Task SeedInitial()
        {
            if (await _dbContext.AccountHierarchies.CountAsync() == 1)
            {
                var first = await _dbContext.AccountHierarchies.FirstAsync();

                await _dbContext.AccountHierarchies.AddRangeAsync(
                    new AccountHierarchy() { Name = "B", PlacementPreference = 2, ParentId = first.Id, UplinkId = first.Id, LevelPath = "/1/" },
                    new AccountHierarchy() { Name = "C", PlacementPreference = 3, ParentId = first.Id, UplinkId = first.Id, LevelPath = "/2/" },
                    new AccountHierarchy() { Name = "D", PlacementPreference = 3, ParentId = first.Id + 1, UplinkId = first.Id, LevelPath = "/1/1/" },
                    new AccountHierarchy() { Name = "H", PlacementPreference = 1, ParentId = first.Id + 2, UplinkId = first.Id, LevelPath = "/2/1/" },
                    new AccountHierarchy() { Name = "K", PlacementPreference = 3, ParentId = first.Id + 2, UplinkId = first.Id, LevelPath = "/2/2/" },
                    new AccountHierarchy() { Name = "F", PlacementPreference = 2, ParentId = first.Id + 1, UplinkId = first.Id, LevelPath = "/1/2/" },
                    new AccountHierarchy() { Name = "G", PlacementPreference = 3, ParentId = first.Id + 3, UplinkId = first.Id, LevelPath = "/1/1/1/" },
                    new AccountHierarchy() { Name = "V", PlacementPreference = 3, ParentId = first.Id + 3, UplinkId = first.Id, LevelPath = "/1/1/2/" },
                    new AccountHierarchy() { Name = "L", PlacementPreference = 3, ParentId = first.Id + 5, UplinkId = first.Id, LevelPath = "/2/2/1/" }
                );
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task SeedMultiple(int numOfEntities, string namePrefix)
        {
            var existingIds = await _dbContext.AccountHierarchies
                .OrderBy(a => a.Id)
                .Select(a => a.Id)
                .ToListAsync();

            var countOfchildrenDict = new Dictionary<long, int>();
            var allParentIds = await _dbContext.AccountHierarchies.Where(a => a.ParentId != null).Select(a => a.ParentId).ToListAsync();
            foreach (long id in allParentIds)
            {
                if(!countOfchildrenDict.ContainsKey(id))
                {
                    countOfchildrenDict[id] = 0;
                }

                countOfchildrenDict[id] += 1;
            }

            for (int i = 0; i < numOfEntities; i++)
            {
                var random = new Random();

                long uplinkId = existingIds[random.Next(0,existingIds.Count)];
                //long? parentId = ChooseParent(existingIds, countOfchildrenDict);

                var placementOptions = new int[]{1,2,3,3,3,3,3};    // prefer more autoplacement 
                var added = await AddNode(new AccountHierarchy
                {
                    Name = namePrefix + (i+1),
                    PlacementPreference = placementOptions[random.Next(0,placementOptions.Length)],
                    UplinkId = uplinkId,
                }, false);

                // if(parentId != null)
                // {
                //     if(!countOfchildrenDict.ContainsKey((long)parentId))
                //     {
                //         countOfchildrenDict[(long)parentId] = 0;
                //     }
                //     countOfchildrenDict[(long)parentId] += 1;
                // }

                if(i % 1000 == 0)
                {
                    System.Console.WriteLine("Processed batch of 1000");
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        private async Task<AccountHierarchy> AddToParent(AccountHierarchy entity, long? parentId, bool autoCommit = true)
        {
            if (!parentId.HasValue)
            {
                SqlHierarchyId parentLevel = SqlHierarchyId.GetRoot();
                entity.ParentId = null;
                entity.UplinkId = null;
                entity.LevelPath = parentLevel.ToString();
            }
            else
            {
                var parent = await _dbContext.AccountHierarchies.Where(x => x.Id == parentId)
                    .Include(x => x.Children)
                    .FirstOrDefaultAsync();
                    
                SqlHierarchyId parentLevel = SqlHierarchyId.Parse(parent.LevelPath);
                
                
                var lastSibling = parent.Children.OrderByDescending(x => x.LevelPath).FirstOrDefault();
                
                string levelPath = null;
                
                if(entity.Leg == 2)
                {
                    if (lastSibling == null)
                    {
                        levelPath = parent.LevelPath + "2/";
                    }
                    else
                    {
                        SqlHierarchyId newLevel = parentLevel.GetDescendant(SqlHierarchyId.Parse(lastSibling.LevelPath), SqlHierarchyId.Null);
                        levelPath = newLevel.ToString();
                    }
                }
                else if(entity.Leg == 1)   // in any other case - put it on the left
                {
                    if (lastSibling == null)
                    {
                        levelPath = parent.LevelPath + "1/";
                    }
                    else
                    {
                        SqlHierarchyId newLevel = parentLevel.GetDescendant(SqlHierarchyId.Null, SqlHierarchyId.Parse(lastSibling.LevelPath));
                        levelPath = newLevel.ToString();
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Leg", "Invalid value for leg: " + entity.Leg);
                }

                entity.LevelPath = levelPath;
                entity.ParentId = parentId;
            }

            var result = (await _dbContext.AccountHierarchies.AddAsync(entity)).Entity;

            if(autoCommit)
            {
                await _dbContext.SaveChangesAsync();
            }

            return result;
        }

        public async Task<AccountHierarchy> AddNode(AccountHierarchy entity, bool autoCommit = true)
        {
            var sponsor = await _dbContext.AccountHierarchies
                .Include(a => a.Children).FirstOrDefaultAsync(a => a.Id == entity.UplinkId);
            if(sponsor.Children.Count == 0)
            {
                entity.ParentId = sponsor.Id;
                entity.Leg = sponsor.PlacementPreference == 2 ? 2 : 1;
            }
            else
            {
                Stopwatch sw = Stopwatch.StartNew(); 

                var sponsorTree = await _dbContext.AccountHierarchies
                    .OrderBy(a => a.LevelPath.Length)
                    .ThenBy(a => a.LevelPath)
                    .Where(a => a.LevelPath.StartsWith(sponsor.LevelPath))
                    .AsNoTracking()
                    .ToListAsync();

                System.Console.WriteLine("Finding parent for entity " + entity.Name);
                var parent = await Task.Run(() => FindAvailableNodeInLoop(sponsorTree));
                System.Console.WriteLine($"Found parent {parent.Name} for entity {entity.Name}, took {sw.ElapsedMilliseconds} ms");

                entity.ParentId = parent.Id;
                entity.Leg = parent.PlacementPreference;
            }

            var created = await this.AddToParent(entity, entity.ParentId, autoCommit);
            return created;
        }

        public AccountHierarchy FindAvailableNodeInLoop(List<AccountHierarchy> nodes)
        {
            if (nodes.Count == 0)
            {
                throw new ArgumentException("Cannot process empty list of nodes");
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                string nodePath = node.LevelPath;
                string leftPath = nodePath + "1/";
                string rightPath = nodePath + "2/";

                if(node.PlacementPreference == 1)
                {
                    var foundNode = nodes.FirstOrDefault(n => n.LevelPath == leftPath);
                    if(foundNode == null)
                    {
                        return node;
                    }
                }
                else if(node.PlacementPreference == 2)
                {
                    var foundNode = nodes.FirstOrDefault(n => n.LevelPath == rightPath);
                    if(foundNode == null)
                    {
                        return node;
                    }
                }
                else
                {
                    var foundLeft = nodes.FirstOrDefault(n => n.LevelPath == leftPath);
                    var foundRight = nodes.FirstOrDefault(n => n.LevelPath == rightPath);
                    
                    // if at least one is null, set it as a path
                    if (foundLeft == null)
                    {
                        node.PlacementPreference = 1;
                        return node;
                    }
                    else if (foundRight == null)
                    {
                        node.PlacementPreference = 2;
                        return node;
                    }
                }
            }

            throw new InvalidOperationException("Could not find proper parent");
        }


        private long? ChooseParent(IList<long> ids, Dictionary<long, int> parentsDict)
        {
            if(ids.Count == 0)
            {
                return null;
            }

            var random = new Random();

            int rand = random.Next(0, ids.Count);
            var targetId = ids[rand];

            int counter = 0;

            while (counter < ids.Count && parentsDict.ContainsKey(targetId) && parentsDict[targetId] >= 2)
            {
                rand = random.Next(0, ids.Count);
                targetId = ids[rand];
                counter++;
            }

            return targetId;
        }
    }
}