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
            var accountsFirst = await _dbContext.Accounts.OrderBy(a => a.Id).FirstOrDefaultAsync();
            var accountsLast = await _dbContext.Accounts.OrderBy(a => a.Id).LastOrDefaultAsync();
            var accCount = await _dbContext.Accounts.CountAsync();

            var accountsHierarchiesFirst = await _dbContext.AccountHierarchies.OrderBy(a => a.Id).FirstOrDefaultAsync();
            var accountsHierarchiesLast = await _dbContext.AccountHierarchies.OrderBy(a => a.Id).LastOrDefaultAsync();
            var hierCount = await _dbContext.AccountHierarchies.CountAsync();

            var treeBuilder = new TreeBuilder(_dbContext);
            treeBuilder.CalculateBonuses(accountsHierarchiesFirst);

            return View(new IndexViewModel { 
                Accounts = new List<Account>(){accountsFirst, accountsLast},
                AccountHierarchies = new List<AccountHierarchy>(){ accountsHierarchiesFirst, accountsHierarchiesLast},
                CountRegular = accCount,
                CountHierarchies = hierCount });
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
        public async Task<IActionResult> CreateHierarchy(int sponsorId, string name, int placement)
        {
            var newAccountHierarchy = new AccountHierarchy
            {
                Name = name,
                PlacementPreference = placement,
                UplinkId = sponsorId
            };
            var hierarchyRepo = new HierarchyRepository(_dbContext);

            var entity = await hierarchyRepo.AddNode(newAccountHierarchy);
            System.Console.WriteLine($"Created hierarchy: {entity.Name}, parent: {entity.ParentId}, level: {entity.LevelPath}");

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetTree()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetTree(int? parentId)
        {
            Account root = parentId.HasValue ? 
                await _dbContext.Accounts.FindAsync((long)parentId) :
                await _dbContext.Accounts.OrderBy(a => a.Id).FirstOrDefaultAsync();

            if (root == null)
            {
                return View("GetTree");
            }

            TreeBuilder treeBuilder = new TreeBuilder(_dbContext);
            var account = treeBuilder.BuildTreeInMemory(root);

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

        [HttpPost]
        public async Task<IActionResult> GetHierarchyTree(int? parentId)
        {
            AccountHierarchy root = parentId.HasValue ? 
                await _dbContext.AccountHierarchies.FindAsync((long)parentId) :
                await _dbContext.AccountHierarchies.OrderBy(a => a.Id).FirstOrDefaultAsync();

            TreeBuilder treeBuilder = new TreeBuilder(_dbContext);
            //var account = treeBuilder.BuildHierarchyTreeRecursive(root);
            var account = treeBuilder.BuildHierarchyTreeRecursive(root);

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

        public async Task<IActionResult> GetRandomTree()
        {
            var total = await _dbContext.Accounts.CountAsync();
            var rand = new Random();
            
            var selection = _dbContext.Accounts.OrderBy(a => a.Id).Skip(rand.Next(0, total));
            Account root = await selection.FirstOrDefaultAsync();
            
            TreeBuilder treeBuilder = new TreeBuilder(_dbContext);
            var account = treeBuilder.BuildTreeInMemory(root);

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
            ViewData["total"] = await selection.CountAsync();
            ViewData["first"] = root.Id;
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
