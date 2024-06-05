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

namespace ASPFinalProject.Controllers.Test
{
    // [Authorize(Roles = "Teacher")]
    public class TeacherTestsController : Controller
    {
        private readonly ExamDbContext _context;
        private readonly UserManager<User> _userManager;

        public TeacherTestsController(ExamDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Tests
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }
            var examDbContext = _context.Tests
                .Where(t => t.Author.Id == currentUser.Id)
                .Include(t => t.Author);
            return View(await examDbContext.ToListAsync());
        }

        // GET: Tests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                .Include(t => t.Author)
                .FirstOrDefaultAsync(m => m.TestId == id);
            if (test == null || test.Author.Id != _userManager.GetUserAsync(User).Id)
            {
                return NotFound();
            }

            return View(test);
        }

        // GET: Tests/Create
        public IActionResult Create()
        {
            return View("~/Views/Tests/TeacherTests/Create.cshtml");
        }

        // POST: Tests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TestDTO testDTO)
        {
            if (ModelState.IsValid)
            {
                Models.Test test = new()
                {
                    TestTitle = testDTO.TestTitle,
                    Description = testDTO.Description,
                    IsOpen = testDTO.IsOpen,
                    Password = testDTO.Password,
                    Time = testDTO.Time,
                    NumberOfQuestion = testDTO.NumberOfQuestion
                };
                _context.Add(test);
                await _context.SaveChangesAsync();
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
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Fullname", test.AuthorId);
            return View(test);
        }

        // POST: Tests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TestDTO testDTO)
        {
            var test = await _context.Tests
                        .Include(t => t.Author)
                        .FirstOrDefaultAsync(m => m.TestId == id);
            if (test == null || id != test.TestId)
            {
                return NotFound();
            }
            if (test.AuthorId != _userManager.GetUserAsync(User).Id)
            {
                return Forbid();
            }
            test.TestTitle = testDTO.TestTitle;
            test.Description = testDTO.Description;
            test.IsOpen = testDTO.IsOpen;
            test.Password = testDTO.Password;
            test.Time = testDTO.Time;
            test.NumberOfQuestion = testDTO.NumberOfQuestion;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(test);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TestExists(test.TestId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Fullname", test.AuthorId);
            return View(test);
        }

        // GET: Tests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = await _context.Tests
                .Include(t => t.Author)
                .FirstOrDefaultAsync(m => m.TestId == id);
            if (test == null || test.AuthorId != _userManager.GetUserAsync(User).Id)
            {
                return NotFound();
            }

            return View(test);
        }

        // POST: Tests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if(TestExists(id))
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
            if(test == null)
            {
                return NotFound();
            } else if(user == null || user.Id != test.AuthorId)
            {
                return Forbid();
            }
            var questionList = await _context.Questions.Where(q => q.TestId == test.TestId).ToListAsync();
            if(questionList.Count < test.NumberOfQuestion || questionList == null)
            {
                return BadRequest("You haven't add enough question. Required: " + test.NumberOfQuestion);
            }
            test.IsOpen = true;
            _context.Update(test);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetResult(int testId)
        {
            var test = await _context.Tests.FindAsync(testId);
            if( test == null )
            {
                return NotFound();
            }
            var user = _userManager.GetUserAsync(User);
            if(user == null || user.Id != test.AuthorId)
            {
                return Forbid();
            }
            var resultList = await _context.Results.Where(r => r.TestId == testId).ToListAsync();
            return View(resultList);
        }

        private bool TestExists(int id)
        {
            return _context.Tests.Any(e => e.TestId == id);
        }
    }
}
