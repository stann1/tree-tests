using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetworkTreeWebApp.Models;

namespace NetworkTreeWebApp.Data
{
    public class AccountRepository
    {
        private readonly AccountsContext _dbContext;

        public AccountRepository(AccountsContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async void ClearAll()
        {
            await _dbContext.Database.ExecuteSqlRawAsync("DELETE FROM Accounts");
        }

        public async Task Seed(int numOfEntities, string namePrefix)
        {
            int count = await _dbContext.Accounts.CountAsync();
            int processed = 0;

            int next = Math.Min(numOfEntities, count);
            int remaining = numOfEntities;

            while (processed < numOfEntities)
            {

                int result = await SeedBatch(next, namePrefix);
                //int result = Math.Min(next, remaining);
                System.Console.WriteLine("Inserting next: " + result);
                
                processed += result;
                next += result;
                remaining = numOfEntities - processed;
            }

            System.Console.WriteLine("processed: " + processed);
        }

        private async Task<int> SeedBatch(int numOfEntities, string namePrefix)
        {
            if(numOfEntities < 1 || string.IsNullOrEmpty(namePrefix))
            {
                throw new ArgumentException("Invalid number or prefix");
            }
            
            var accounts = new List<Account>();

            var countOfchildrenDict = new Dictionary<long, int>();
            var allParentIds = await _dbContext.Accounts.Where(a => a.ParentId != null).Select(a => a.ParentId).ToListAsync();
            foreach (long id in allParentIds)
            {
                if(!countOfchildrenDict.ContainsKey(id))
                {
                    countOfchildrenDict[id] = 0;
                }

                countOfchildrenDict[id] += 1;
            }

            var existingIds = await _dbContext.Accounts
                .OrderBy(a => a.Id)
                .Select(a => a.Id)
                .ToListAsync();

            var random = new Random();

            for (int i = 0; i < numOfEntities; i++)
            {
                long? parentId = ChooseParent(existingIds, countOfchildrenDict);
                long uplinkId = existingIds[random.Next(0,existingIds.Count)];
                
                accounts.Add(new Account
                {
                    Name = namePrefix + (i+1),
                    PlacementPreference = random.Next(1,4),
                    Leg = random.Next(1,3),
                    UplinkId = uplinkId,
                    ParentId = parentId
                });
                
                if(parentId != null)
                {
                    if(!countOfchildrenDict.ContainsKey((long)parentId))
                    {
                        countOfchildrenDict[(long)parentId] = 0;
                    }
                    countOfchildrenDict[(long)parentId] += 1;
                }
            }

            if(accounts.Count > 0)
            {
                await _dbContext.AddRangeAsync(accounts);
                await _dbContext.SaveChangesAsync();
            }

            return accounts.Count;
        }

        public async Task<long> AddNode(Account entity)
        {
            var sponsor = await _dbContext.Accounts.Include(a => a.Children).FirstOrDefaultAsync(a => a.Id == entity.UplinkId);
            if(sponsor.Children.Count == 0)
            {
                entity.ParentId = sponsor.Id;
                entity.Leg = sponsor.PlacementPreference;
            }
            else
            {
                Stopwatch sw = Stopwatch.StartNew(); 
                System.Console.WriteLine("Finding parent for entity " + entity.Name);
                var parent = await FindAvailableNodeRecursive(sponsor.Children, sponsor.PlacementPreference);
                System.Console.WriteLine($"Found parent {parent.Name} for entity {entity.Name}, took {sw.ElapsedMilliseconds} ms");

                entity.ParentId = parent.Id;
                entity.Leg = parent.PlacementPreference;
            }

            var created = (await _dbContext.Accounts.AddAsync(entity)).Entity;
            await _dbContext.SaveChangesAsync();
            return created.Id;
        }

        public async Task<Account> FindAvailableNodeRecursive(List<Account> nodes, int placementPreference)
        {
            if (nodes.Count == 1)
            {
                return nodes[0];
            }
            
            Account leftLeg = null;
            Account rightLeg = null;

            var leftNode = nodes.FirstOrDefault(n => n.Leg == 1);
            var leftChildren = await _dbContext.Accounts.Where(a => a.ParentId == leftNode.Id).ToListAsync();
            if(leftChildren.Count == 0)
            {
                leftLeg = leftNode;
                if(placementPreference == 1)
                {
                    return leftLeg;
                }
            }
            else
            {
                leftLeg = await FindAvailableNodeRecursive(leftChildren, nodes[0].PlacementPreference);
            }

            var rightNode = nodes.FirstOrDefault(n => n.Leg == 2);
            var rightChildren = await _dbContext.Accounts.Where(a => a.ParentId == rightNode.Id).ToListAsync();
            if(rightChildren.Count == 0)
            {
                rightLeg = rightNode;
                if(placementPreference == 2)
                {
                    return rightLeg;
                }
            }
            else
            {
                rightLeg = await FindAvailableNodeRecursive(rightChildren, nodes[1].PlacementPreference);
            }
            
            switch (placementPreference)
            {
                case 1: return leftLeg;
                case 2: return rightLeg;
                case 3:
                    return leftLeg.Id < rightLeg.Id ? leftLeg : rightLeg;            
                default:
                    return leftLeg.Id < rightLeg.Id ? leftLeg : rightLeg;
            }
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

        private long? ChooseParentSimple(IList<long> ids)
        {
            if(ids.Count == 0)
            {
                return null;
            }

            var random = new Random();

            int rand = random.Next(0, ids.Count);
            var targetId = ids[rand];

            return targetId;
        }
    }
}