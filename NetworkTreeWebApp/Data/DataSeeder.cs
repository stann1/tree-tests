using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace NetworkTreeWebApp.Data
{
    public class DataSeeder
    {
        private readonly AccountsContext _dbContext;

        public DataSeeder(AccountsContext dbContext)
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

            while (processed < numOfEntities)
            {

                //int result = await SeedBatch(next, namePrefix);
                int result = next;
                System.Console.WriteLine("Inserting next: " + next);
                
                processed += result;
                next += result;
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
                .Take(numOfEntities / 2)
                .ToListAsync();

            var random = new Random();

            for (int i = 0; i < numOfEntities; i++)
            {
                long? parentId = ChooseParent(existingIds, countOfchildrenDict);
                
                accounts.Add(new Account
                {
                    Name = namePrefix + (i+1),
                    PlacementPreference = random.Next(1,4),
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

        public async Task<bool> AddToParent(Account entity, Account sponsorEntity)
        {
            var sponsorChildren = await _dbContext.Accounts.Include(a => a.Children).Where(a => a.ParentId == sponsorEntity.Id).ToListAsync();
            if(sponsorChildren.Count == 0)
            {
                // add and return
                return true;
            }

            return false;
        }


        private long? ChooseParent(IList<long> ids, Dictionary<long, int> parentsDict)
        {
            if(ids.Count == 0)
            {
                return null;
            }

            var random = new Random();

            int rand = random.Next((int)ids[0], ids.Count);
            var targetId = ids[rand];

            while (parentsDict.ContainsKey(targetId) && parentsDict[targetId] >= 2)
            {
                rand = random.Next((int)ids[0], ids.Count);
                targetId = ids[rand];
            }

            return ids[rand];
        }
    }
}