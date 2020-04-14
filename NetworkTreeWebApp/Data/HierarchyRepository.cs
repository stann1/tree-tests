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

        public async Task<AccountHierarchy> Add(AccountHierarchy entity, long? parentId)
        {
            SqlHierarchyId parentItem;
            AccountHierarchy lastItemInCurrentLevel;

            if (!parentId.HasValue)
            {
                parentItem = SqlHierarchyId.GetRoot();
            }
            else
            {
                parentItem = (await _dbContext.AccountHierarchies.FirstOrDefaultAsync(x => x.Id == parentId)).Level;
            }

            lastItemInCurrentLevel = await _dbContext.AccountHierarchies
                  .Where(x => x.Level.GetAncestor(1).Equals(parentItem))
                  .OrderByDescending(x => x.Level)
                  .FirstOrDefaultAsync();

            SqlHierarchyId child1Level = lastItemInCurrentLevel != null ? lastItemInCurrentLevel.Level : SqlHierarchyId.Null;

            var newLevel = parentItem.GetDescendant(child1Level, SqlHierarchyId.Null);
            entity.Level = newLevel;

            var result = (await _dbContext.AccountHierarchies.AddAsync(entity)).Entity;
            await _dbContext.SaveChangesAsync();

            return result;
        }
    }
}