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
            var random = new Random();

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
                long uplinkId = existingIds[random.Next(0,existingIds.Count)];
                long? parentId = ChooseParent(existingIds, countOfchildrenDict);

                var added = await AddToParent(new AccountHierarchy
                {
                    Name = namePrefix + (i+1),
                    PlacementPreference = random.Next(1,4),
                    Leg = random.Next(1,3),
                    UplinkId = uplinkId,
                    ParentId = parentId
                }, parentId, false);

                if(parentId != null)
                {
                    if(!countOfchildrenDict.ContainsKey((long)parentId))
                    {
                        countOfchildrenDict[(long)parentId] = 0;
                    }
                    countOfchildrenDict[(long)parentId] += 1;
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<AccountHierarchy> AddToParent(AccountHierarchy entity, long? parentId, bool autoCommit = true)
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
                    
                var lastSibling = parent.Children.OrderByDescending(x => x.LevelPath).FirstOrDefault();

                SqlHierarchyId child1Level = lastSibling != null ? SqlHierarchyId.Parse(lastSibling.LevelPath) : SqlHierarchyId.Null;
                SqlHierarchyId parentLevel = SqlHierarchyId.Parse(parent.LevelPath);

                var newLevel = parentLevel.GetDescendant(child1Level, SqlHierarchyId.Null);
                entity.LevelPath = newLevel.ToString();
                entity.ParentId = parentId;

                if(entity.UplinkId == null)
                {
                    entity.UplinkId = parentId;
                }
            }

            var result = (await _dbContext.AccountHierarchies.AddAsync(entity)).Entity;

            if(autoCommit)
            {
                await _dbContext.SaveChangesAsync();
            }

            return result;
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