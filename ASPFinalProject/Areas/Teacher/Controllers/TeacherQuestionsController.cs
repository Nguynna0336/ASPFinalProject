using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ASPFinalProject.Models;
using Microsoft.AspNetCore.Authorization;
using ASPFinalProject.DTOs.Question;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Identity;
using Org.BouncyCastle.Crypto.Modes.Gcm;
using PagedList.Core;
using System.Drawing.Printing;

namespace ASPFinalProject.Controllers.QuestionController
{
    [Authorize(Roles = "Teacher")]
    [Area("Teacher")]
    public class TeacherQuestionsController : Controller
    {
        private readonly ExamDbContext _context;
        private readonly INotyfService _notyfService;
        private readonly UserManager<User> _userManager;

        public TeacherQuestionsController(ExamDbContext context, INotyfService notyfService, UserManager<User> userManager)
        {
            _context = context;
            _notyfService = notyfService;
            _userManager = userManager;
        }

        // GET: TeacherQuestions
        public async Task<IActionResult> Index()
        {
            var examDbContext = _context.Questions.Include(q => q.Test);
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
                .Include(q => q.Test)
                .FirstOrDefaultAsync(m => m.QuestionsId == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        // GET: TeacherQuestions/Create
        public async Task<IActionResult> Create([FromRoute(Name = "id")] int TestId)
        {
            var test = await _context.Tests.FindAsync(TestId);
            if (test == null)
            {
                _notyfService.Error("Cannot find test with id: " + TestId);
                return RedirectToAction("Index", "TeacherTests");
            }
            else
            {
                if(test.CurrentQuestions >= test.NumberOfQuestion)
                {
                    _notyfService.Error("Cannot add more questions");
                    return RedirectToAction("Index", "TeacherTests");
                }
                ViewBag.TestId = test.TestId;
                ViewBag.NumberOfQuestion = test.NumberOfQuestion;
                return View();
            }
        }

        // POST: TeacherQuestions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromRoute(Name = "id")] int testId, List<QuestionDTO> questionDTOs)
        {
            if (ModelState.IsValid)
            {
                int count = 0;
                var test = await _context.Tests.FindAsync(testId);
                if (test == null)
                {
                    _notyfService.Error("Cannot find test with id: " + testId);
                    return RedirectToAction("Index", "TeacherTests");
                }
                List<QuestionDTO> error = new List<QuestionDTO>();
                foreach (QuestionDTO questionDTO in questionDTOs)
                {
                    try
                    {
                        if(test.CurrentQuestions < test.NumberOfQuestion)
                        {
                            Question question = new()
                            {
                                Description = questionDTO.Description,
                                OptionA = questionDTO.OptionA,
                                OptionB = questionDTO.OptionB,
                                OptionC = questionDTO.OptionC,
                                OptionD = questionDTO.OptionD,
                                Answer = questionDTO.Answer,
                                TestId = testId
                            };
                            _context.Add(question);
                            test.CurrentQuestions++;
                            await _context.SaveChangesAsync();
                            count++;
                        } else
                        {
                            _notyfService.Error("Cannot add more question");
                            return RedirectToAction(nameof(getQuestions), new { id = testId });                         
                        }
                    }
                    catch (Exception ex)
                    {
                        error.Add(questionDTO);
                    }
                }
                _context.Update(test);
                await _context.SaveChangesAsync();
                if (error.Count > 0)
                {
                    _notyfService.Error($"{count} questions have been added, {error.Count} questions couldn't be added, please try again");
                    return View(error);
                }
                else
                {
                    _notyfService.Success($"{count} questions have been added");
                    return RedirectToAction(nameof(getQuestions), new { id = testId });
                }
            }
            return View(questionDTOs);
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
            return View(question);
        }

        // POST: TeacherQuestions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, QuestionDTO questionDTO)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var question = await _context.Questions.FindAsync(id);
                    if (question == null)
                    {
                        _notyfService.Error("Cannot find question with id: " + id);
                        return View(questionDTO);
                    }
                    question.Description = questionDTO.Description;
                    question.OptionA = questionDTO.OptionA;
                    question.OptionB = questionDTO.OptionB;
                    question.OptionC = questionDTO.OptionC;
                    question.OptionD = questionDTO.OptionD;
                    question.Answer = questionDTO.Answer;

                    _context.Update(question);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _notyfService.Error(ex.Message);
                    return View(questionDTO);
                }
                _notyfService.Success("Question has been edited successfully");
                return RedirectToAction(nameof(Index));
            }
            _notyfService.Error("Please check your data again");
            return View(questionDTO);
        }

        // GET: TeacherQuestions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
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
            var user = _userManager.GetUserAsync(User);
            var question = await _context.Questions.Include(t => t.Test).FirstOrDefaultAsync(q => q.QuestionsId == id);
            if (question == null)
            {
                _notyfService.Error("Cannot find question with id: " + id);
                return View();
            }
            else if (question.Test.AuthorId != user.Id)
            {
                _notyfService.Warning("You dont have permission to delete this question. Only author of its test can delete this questions");
                return RedirectToAction(nameof(Index));
            }
            if (question != null)
            {
                _context.Questions.Remove(question);
            }
            await _context.SaveChangesAsync();
            _notyfService.Success("You have deleted this successfully");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> getQuestions([FromRoute(Name = "id")] int testId, int page =1)
        {
            var pageNumber = page;
            var test = await _context.Tests.FindAsync(testId);
            if (test == null)
            {
                _notyfService.Error("Cannot find test with id: " + testId);
                return RedirectToAction("Index", "TeacherTests");
            }
            List<Question> questions = await _context.Questions.Where(q => q.TestId == testId).ToListAsync();
            PagedList<Question> model = new PagedList<Question> (questions.AsQueryable(), pageNumber, 10);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = 10;
            return View(model);
        }
        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.QuestionsId == id);
        }
    }
}
