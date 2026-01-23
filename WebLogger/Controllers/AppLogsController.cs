using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PcsSelcomWebLogger.Models;
using PcsSelcomWebLogger.Data;
using System.Diagnostics.Eventing.Reader;
using Microsoft.IdentityModel.Tokens;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PcsSelcomWebLogger.Services;
using Microsoft.AspNetCore.Authorization;

namespace PcsSelcomWebLogger.Controllers
{
    [Authorize(Roles = "User, Administrator")]
    public class AppLogsController : Controller
    {
        private readonly AppLogsContext _context;
        private readonly ILogBufferService _logBuffer;

        public AppLogsController(AppLogsContext context, ILogBufferService logBuffer)
        {
            _context = context;
            _logBuffer = logBuffer;
        }
        //Get
        [HttpGet]
        public async Task<IActionResult> Index(string searchString, string logtype, DateTime? from, DateTime? to)
        {
            var Logs = _context.AppLogs.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                Logs = Logs.Where(x => x.AppName == searchString);
            }
            if (!string.IsNullOrEmpty(logtype))
            {
                Logs = Logs.Where(x => x.LogType == logtype);
            }
            if (from.HasValue)
            {
                Logs = Logs.Where(x => x.DateTime >= from.Value);
            }
            if (to.HasValue)
            {
                Logs = Logs.Where(x => x.DateTime <= to.Value);
            }
            var logs = new AppLogsFilterViewModel();
            logs.Logs = await Logs.ToListAsync();
            // data
            logs.SearchString = searchString;
            logs.LogType = logtype;
            logs.From = from;
            logs.To = to; //
            return View(logs);
            
        }
        [HttpGet]
        public ActionResult Details(int? Id)
        {
            if (Id == null)
            {
                return NotFound(); // Do sprawdzenia
            }
            var log = _context.AppLogs.FirstOrDefault(x => x.Id == Id);
            if (log == null)
            {
                return NotFound(); // Log not found
            }
            return View(log);
        }
        [HttpGet]
        [Authorize(Roles = "Administrator")]
		public IActionResult Create()
        {
            return View();
        }
        [HttpPost, ActionName("Create")]
        [Authorize(Roles = "Administrator")]
        public IActionResult Create([FromForm] AppLogs newLog) //Można Dodać Async/Await 
        {

            if (ModelState.IsValid)
            {
                _logBuffer.Add(newLog);

                return RedirectToAction("Index", "AppLogs");
            }
            else
            {
                return View(newLog);
            }
        }
        [HttpPost, ActionName("CreateJson")]
        [Authorize(Roles ="Administrator")]
        public IActionResult CreateJson([FromBody] AppLogs newLog)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);  
            _logBuffer.Add(newLog);

            return Ok(newLog);  
        }
        [HttpGet]
        public async Task<IActionResult> GetJson()
        {
            var logs = await _context.AppLogs
                .OrderByDescending(l => l.DateTime)
                .ToListAsync();

            return Json(logs);
        }
        [HttpGet]
        public IActionResult Edit(int? Id)
        {
            if (Id == null)
            {
                return NotFound(); // Do sprawdzenia
            }
            var log = _context.AppLogs.FirstOrDefault(x => x.Id == Id);
            if (log == null)
            {
                return NotFound(); // Log not found
            }
            return View(log);
        }
        [HttpPost, ActionName("Edit")]
        [Authorize(Roles = "Administrator")]
        public IActionResult Edit(AppLogs log)
        {
            if (!ModelState.IsValid)
                return View(log);

            _context.Update(log);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult Delete(int? Id)
        {
            if (Id == null)
            {
                return NotFound(); // Do sprawdzenia
            }
            var log = _context.AppLogs.FirstOrDefault(x => x.Id == Id);
            if (log == null)
            {
                return NotFound(); 
            }
            return View(log); 
        }
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrator")]
        public IActionResult DeleteConfirm(int Id)
        {
            var log = _context.AppLogs.FirstOrDefault(x => x.Id == Id);
            if (log == null)
            {
                return NotFound();
            }
            else
            {
                _context.AppLogs.Remove(log);
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult AddDaily()
        {
            AppLogs newlog = new AppLogs
            {
                AppName = "DailyLog",
                DateTime = DateTime.UtcNow,
                LogType = "Information",
                Content = "Today"
            };

            _logBuffer.Add(newlog);
            return RedirectToAction("Index");
        }

        [HttpPost, ActionName("Refresh")]
        public async Task<IActionResult> Refresh()
        {
            var savedLogs = _logBuffer.GetAllAndClear();
            foreach (var mylog in savedLogs)
            {
                await _context.AppLogs.AddAsync(mylog);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

    }
}
