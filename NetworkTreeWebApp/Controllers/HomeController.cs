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
            var dataSeeder = new DataSeeder(_dbContext);
            await dataSeeder.Seed(number, prefix);
            System.Console.WriteLine("Done.");
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> GetTree()
        {
            var accounts = await _dbContext.Accounts.ToListAsync();

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
            var account = TreeBuilder.BuildTree(accounts);

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

            var randAccounts = await _dbContext.Accounts.OrderBy(a => a.Id).Skip(rand.Next(1, total)).ToListAsync();

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
            var account = TreeBuilder.BuildTree(randAccounts);

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
