using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASPFinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using ASPFinalProject.DTOs.Test;
using AspNetCoreHero.ToastNotification.Abstractions;
using PagedList.Core;
using System.Drawing.Printing;

namespace ASPFinalProject.Areas.Teacher.Controllers
{
    [Authorize(Roles = "Teacher, Admin")]
    [Area("Teacher")]
    public class TeacherTestsController : Controller
    {


        private readonly ExamDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly INotyfService _notyfService;

        public TeacherTestsController(ExamDbContext context, UserManager<User> userManager, INotyfService notyfService)
        {
            _context = context;
            _userManager = userManager;
            _notyfService = notyfService;
        }

        // GET: Tests
        public async Task<IActionResult> Index(int page = 1)
        {
            var pageNumber = page;
            var pageSize = 5;
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }
            List<Test> tests = new List<Test>();
            tests = await _context.Tests
                .Where(t => t.Author.Id == currentUser.Id)
                .Include(t => t.Author).ToListAsync();
            PagedList<Test> models = new PagedList<Test>(tests.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            return View(models);
        }

        // GET: Tests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                .Include(t => t.Author)
                .FirstOrDefaultAsync(m => m.TestId == id);
            if (test == null || test.Author.Id != currentUser!.Id)
            {
                return NotFound();
            }

            return View(test);
        }

        // GET: Tests/Create
        public IActionResult Create()
        {
            /*TestDTO testDTO = new TestDTO();*/
            return View();
        }

        // POST: Tests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestDTO testDTO)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var user = _userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                Models.Test test = new()
                {
                    TestTitle = testDTO.TestTitle,
                    Description = testDTO.Description,
                    IsOpen = testDTO.IsOpen,
                    Password = testDTO.Password,
                    Time = testDTO.Time,
                    NumberOfQuestion = testDTO.NumberOfQuestion,
                    AuthorId = currentUser!.Id
                };
                _context.Add(test);
                await _context.SaveChangesAsync();
                _notyfService.Success("Test has been created");
                return RedirectToAction(nameof(Index));
            }
            return View(testDTO);
        }

        // GET: Tests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests.FindAsync(id);
            if (test == null)
            {
                return NotFound();
            }
            TestDTO testdto = new TestDTO
            {
                TestTitle = test.TestTitle,
                Description = test.Description,
                IsOpen = test.IsOpen,
                Password = test.Password,
                Time = test.Time,
                NumberOfQuestion = test.NumberOfQuestion
            };
            return View(testdto);
        }

        // POST: Tests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TestDTO testDTO)
        {
            if (ModelState.IsValid)
            {
                var test = await _context.Tests
                        .Include(t => t.Author)
                        .FirstOrDefaultAsync(m => m.TestId == id);
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _notyfService.Information("Please login before edit");
                    return Challenge();
                }
                if (test == null || id != test.TestId)
                {
                    return NotFound();
                }
                if (test.AuthorId != user.Id)
                {
                    _notyfService.Error("You dont have permission to edit this");
                    return RedirectToAction(nameof(Index));
                }
                test.TestTitle = testDTO.TestTitle;
                test.Description = testDTO.Description;
                test.IsOpen = testDTO.IsOpen;
                test.Password = testDTO.Password;
                test.Time = testDTO.Time;
                test.NumberOfQuestion = testDTO.NumberOfQuestion;
                try
                {
                    _context.Update(test);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    _notyfService.Error("Something go wrong, please try again");
                    return View(testDTO);
                }
                _notyfService.Success("Test's details have been changed");
                return RedirectToAction(nameof(Index));
            }
            _notyfService.Error("Something wrong, check your details again before save");
            return View(testDTO);
        }

        // GET: Tests/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (TestExists(id))
            {
                return NotFound();
            }
            var test = await _context.Tests.FindAsync(id);
            if (test != null && test.AuthorId != _userManager.GetUserAsync(User).Id)
            {
                _context.Tests.Remove(test);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> OpenTest(int? id)
        {
            var test = await _context.Tests.FirstOrDefaultAsync(t => t.TestId == id);
            var user = await _userManager.GetUserAsync(User);
            if (test == null)
            {
                return NotFound();
            }
            else if (user == null || user.Id != test.AuthorId)
            {
                return Forbid();
            }
            var questionList = await _context.Questions.Where(q => q.TestId == test.TestId).ToListAsync();
            if (questionList.Count < test.NumberOfQuestion || questionList == null)
            {
                return BadRequest("You haven't add enough question. Required: " + test.NumberOfQuestion);
            }
            test.IsOpen = true;
            _context.Update(test);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetResult([FromRoute(Name = "id")]int testId,int page = 1)
        {
            int pageNumber = page;
            int pagesize = 10;
            var test = await _context.Tests.FindAsync(testId);
            if (test == null)
            {
                _notyfService.Error("Please try again with a valid test");
                return RedirectToAction(nameof(Index));
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null || user.Id != test.AuthorId)
            {
                _notyfService.Error("You dont have permission to do this");
                return RedirectToAction(nameof(Index));
            }
            List<Result> results = await _context.Results.Include(r => r.User).Where(r => r.TestId == testId).ToListAsync();
            PagedList<Result> model = new PagedList<Result>(results.AsQueryable(),pageNumber, pagesize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pagesize;
            ViewBag.TestTitle = test.TestTitle;
            ViewBag.TestId = test.TestId;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SetStatus([FromRoute(Name = "id")] int testId)
        {
            var test = await _context.Tests.FindAsync(testId);
            if (test == null)
            {
                _notyfService.Error("Cannot find test with id: " + testId.ToString());
            }

            test!.IsOpen =  test!.IsOpen == true ? false : true;
            try
            {
                _context.Update(test);
                await _context.SaveChangesAsync();
                if (test.IsOpen)
                {
                _notyfService.Success("Test is open now");
                } else
                {
                    _notyfService.Success("Test is close now");
                }
            }
            catch (Exception ex)
            {
                _notyfService.Error($"Error: {ex.Message}");

            }
            return RedirectToAction("Details", new { id = testId });
        }
        private bool TestExists(int id)
        {
            return _context.Tests.Any(e => e.TestId == id);
        }
    }
}
