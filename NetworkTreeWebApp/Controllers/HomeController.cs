using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetworkTreeWebApp.Data;
using NetworkTreeWebApp.Models;
using NetworkTreeWebApp.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NetworkTreeWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AccountsContext _dbContext;

        public HomeController(ILogger<HomeController> logger, AccountsContext dbContext)
        {
            _logger = logger;
            this._dbContext = dbContext;
        }

        public async Task<IActionResult> Index()
        {
            var accounts = await _dbContext.Accounts.ToListAsync();
            return View(accounts);
        }

        [HttpPost]
        public async Task<IActionResult> Index(int number, string prefix)
        {
            System.Console.WriteLine("Adding number of accounts: " + number);
            var dataSeeder = new AccountRepository(_dbContext);
            await dataSeeder.Seed(number, prefix);
            System.Console.WriteLine("Done.");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CreateHierarchyMulti(int number, string prefix)
        {
            System.Console.WriteLine("Adding number of accounts: " + number);
            var dataSeeder = new HierarchyRepository(_dbContext);
            await dataSeeder.SeedMultiple(number, prefix);
            System.Console.WriteLine("Done.");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(int? sponsorId, string name, int placement)
        {
            var repo = new AccountRepository(_dbContext);
            var newAccount = new Account
            {
                Name = name,
                PlacementPreference = placement,
                UplinkId = sponsorId
            };
            
            await repo.AddNode(newAccount);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> CreateHierarchy(int? sponsorId, string name, int placement)
        {
            var newAccountHierarchy = new AccountHierarchy
            {
                Name = name,
                PlacementPreference = placement,
                UplinkId = sponsorId,
                ParentId = sponsorId
            };
            var hierarchyRepo = new HierarchyRepository(_dbContext);

            var entity = await hierarchyRepo.AddToParent(newAccountHierarchy, sponsorId);
            System.Console.WriteLine($"Created hierarchy: {entity.Name}, level: {entity.LevelPath}");

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> GetTree()
        {
            var accounts = await _dbContext.Accounts.OrderBy(a => a.Id).ToListAsync();

            Account root = accounts[0];
            TreeBuilder treeBuilder = new TreeBuilder(_dbContext);
            var account = treeBuilder.BuildTreeInMemory(root, accounts.Skip(1).ToList());

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            string json = JsonConvert.SerializeObject(account, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });
            //Console.WriteLine(json);
            return View("GetTree", json);
        }

        public async Task<IActionResult> GetTreeSql()
        {
            var repo = new AccountRepository(_dbContext);
            Account root = await _dbContext.Accounts.OrderBy(a => a.Id).FirstOrDefaultAsync();

            TreeBuilder treeBuilder = new TreeBuilder(_dbContext);
            AccountDto tree = await treeBuilder.BuildTreeInDbRecursive(root.Id);

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            string json = JsonConvert.SerializeObject(tree, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });
            //Console.WriteLine(json);
            return View("GetTree", json);
        }

        public async Task<IActionResult> GetRandomTree()
        {
            var total = await _dbContext.Accounts.CountAsync();
            var rand = new Random();
            
            var randAccounts = await _dbContext.Accounts.OrderBy(a => a.Id).Skip(rand.Next(1, total)).ToListAsync();
            Account root = randAccounts[0];
            // var accounts = new List<Account>()
            // {
            //     new Account(){ Id = 1, Name = "A", TreePlacement = 1, ParentId = null },
            //     new Account(){ Id = 2, Name = "B", TreePlacement = 1, ParentId = 1 },
            //     new Account(){ Id = 3, Name = "C", TreePlacement = 1, ParentId = 1 },
            //     new Account(){ Id = 4, Name = "D", TreePlacement = 1, ParentId = 2 },
            //     new Account(){ Id = 5, Name = "H", TreePlacement = 1, ParentId = 3 },
            //     new Account(){ Id = 6, Name = "K", TreePlacement = 1, ParentId = 3 },
            //     new Account(){ Id = 7, Name = "F", TreePlacement = 1, ParentId = 2 },
            //     new Account(){ Id = 8, Name = "G", TreePlacement = 1, ParentId = 4 },
            //     new Account(){ Id = 9, Name = "V", TreePlacement = 1, ParentId = 4 },
            //     new Account(){ Id = 10, Name = "L", TreePlacement = 1, ParentId = 6 },
            // };
            TreeBuilder treeBuilder = new TreeBuilder(_dbContext);
            var account = treeBuilder.BuildTreeInMemory(root, randAccounts.Skip(1).ToList());

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            string json = JsonConvert.SerializeObject(account, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            });
            //Console.WriteLine(json);
            ViewData["total"] = randAccounts.Count;
            ViewData["first"] = randAccounts[0].Id;
            return View("GetTree", json);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
