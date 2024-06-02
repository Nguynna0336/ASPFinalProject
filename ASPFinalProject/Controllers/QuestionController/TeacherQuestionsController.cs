using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASPFinalProject.Models;
using Microsoft.AspNetCore.Authorization;

namespace ASPFinalProject.Controllers.QuestionController
{
    [Authorize(Roles = "Teacher")]
    public class TeacherQuestionsController : Controller
    {
        private readonly ExamDbContext _context;

        public TeacherQuestionsController(ExamDbContext context)
        {
            _context = context;
        }

        // GET: TeacherQuestions
        public async Task<IActionResult> Index()
        {
            var examDbContext = _context.Questions.Include(q => q.Document).Include(q => q.Test);
            return View(await examDbContext.ToListAsync());
        }

        // GET: TeacherQuestions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.Document)
                .Include(q => q.Test)
                .FirstOrDefaultAsync(m => m.QuestionsId == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // GET: TeacherQuestions/Create
        public IActionResult Create()
        {
            ViewData["DocumentId"] = new SelectList(_context.Documents, "DocumentId", "DocumentId");
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId");
            return View();
        }

        // POST: TeacherQuestions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("QuestionsId,TestId,Description,OptionA,OptionB,OptionC,OptionD,Answer,DocumentId,DocumentPage")] Question question)
        {
            if (ModelState.IsValid)
            {
                _context.Add(question);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DocumentId"] = new SelectList(_context.Documents, "DocumentId", "DocumentId", question.DocumentId);
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", question.TestId);
            return View(question);
        }

        // GET: TeacherQuestions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }
            ViewData["DocumentId"] = new SelectList(_context.Documents, "DocumentId", "DocumentId", question.DocumentId);
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", question.TestId);
            return View(question);
        }

        // POST: TeacherQuestions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("QuestionsId,TestId,Description,OptionA,OptionB,OptionC,OptionD,Answer,DocumentId,DocumentPage")] Question question)
        {
            if (id != question.QuestionsId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(question);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!QuestionExists(question.QuestionsId))
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
            ViewData["DocumentId"] = new SelectList(_context.Documents, "DocumentId", "DocumentId", question.DocumentId);
            ViewData["TestId"] = new SelectList(_context.Tests, "TestId", "TestId", question.TestId);
            return View(question);
        }

        // GET: TeacherQuestions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.Document)
                .Include(q => q.Test)
                .FirstOrDefaultAsync(m => m.QuestionsId == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // POST: TeacherQuestions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question != null)
            {
                _context.Questions.Remove(question);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.QuestionsId == id);
        }
    }
}
