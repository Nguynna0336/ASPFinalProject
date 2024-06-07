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
using ASPFinalProject.DTOs.Submit;
using AspNetCoreHero.ToastNotification.Abstractions;
using PagedList.Core;
using System.Drawing.Printing;

namespace ASPFinalProject.Controllers.TestController
{
    [Authorize]
    public class TestsController : Controller
    {
        private readonly ExamDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly INotyfService _notyfService;

        public TestsController(ExamDbContext context, UserManager<User> userManager, INotyfService notyfService)
        {
            _context = context;
            _userManager = userManager;
            _notyfService = notyfService;
        }

        // GET: StudentTests
        public async Task<IActionResult> Index(int page = 1)
        {
            var pageNumber = page;
            var pageSize = 10;
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }
            var enrolledTest = _context.Results.Where(t => t.UserId == user.Id);
            List<Test> tests = await _context.Tests.Where(t => enrolledTest.Any(r => r.TestId == t.TestId)).Include(t => t.Author).ToListAsync();
            PagedList<Test> models = new PagedList<Test>(tests.AsQueryable(), pageNumber, pageSize );
            return View(models);
        }
        
        public async Task<IActionResult> Enroll(int page = 1)
        {
            var pageNumber = page;
            var pageSize = 10;
            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                _notyfService.Warning("You have to login before enroll!");
                return Challenge();
            }
            var enrolledTest = _context.Results.Where(t => t.UserId == user.Id);
            ViewBag.UserId = user.Id;
            List<Test> tests = await _context.Tests.Where(t => !enrolledTest.Any(r => r.TestId == t.TestId)).Include(t => t.Author).ToListAsync();
            PagedList<Test> models = new PagedList<Test>(tests.AsQueryable(), pageNumber, pageSize);
            return View(models);
        }

        [HttpPost]
        public async Task<IActionResult> Enroll(int testId, int userId, string? password)
        {
            var user = await _userManager.GetUserAsync(User);
            var test = await _context.Tests.FirstOrDefaultAsync(t => t.TestId == testId);
            if(test == null) {
                return NotFound(); 
            } else if(test.Password != null )
            {
                if(password == null || !test.Password.Equals(password))
                {
                _notyfService.Error("Wrong password, please try again");
                return RedirectToAction(nameof(Enroll));
                }
            }
            if(user == null || userId != user.Id)
            {
                return Forbid();
            }
            var result = await _context.Results.FirstOrDefaultAsync(r => r.TestId == testId && r.UserId == userId);
            if (result != null)
            {
                ModelState.AddModelError(string.Empty, "You have already enroll this exam before");
            }
             result = new() 
            {
                UserId = userId,
                TestId = testId,
                Score = -1
            };
            try
            {
                _context.Add(result);
                await _context.SaveChangesAsync();
                _notyfService.Success("You have enrolled successfully");
                return RedirectToAction(nameof(Index));
            } catch (Exception ex)
            {
                _notyfService.Error($"Error: {ex.Message}");
                return View();
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = _userManager.GetUserAsync(User);
            if(user == null)
            {
                return Challenge();
            }
            var test = await _context.Tests.FirstOrDefaultAsync(t => t.TestId == id);
            var result = await _context.Results.FirstOrDefaultAsync(t =>  t.TestId == id);
            if(test == null)
            {
                return RedirectToAction(nameof(Enroll));
            }
            ViewBag.Test = test;
            ViewBag.Result = result;
            return View(test);
        }

        public async Task<IActionResult> UnEnroll(int testId, int userId)
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.UserId = user.Id;
            if(user == null || user.Id != userId)
            {
                _notyfService.Success(user.Id + "and " + userId);
                return Challenge();
            }
            var result = await _context.Results.FirstOrDefaultAsync(r => r.TestId == testId &&  r.UserId == userId);
            if(result == null)
            {
                return NotFound();
            }
            try
            {
                _context.Results.Remove(result);
                await _context.SaveChangesAsync();
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> StartTest(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _context.Results.FirstOrDefaultAsync(r => r.TestId == id && r.UserId == user.Id);
            var test = await _context.Tests.FirstOrDefaultAsync(t => t.TestId == id);
            if(test == null || !test.IsOpen)
            {
                _notyfService.Error("Test is not available now, try later");
                return RedirectToAction("Index", "Home");
            }
            if(result == null)
            {
                _notyfService.Error("You didnt enroll this test");
                return RedirectToAction("Index", "Home");
            }
            var questionList = await _context.Questions.Where(q => q.TestId == id)
                                                        .Select(q => new {q.Description, q.OptionA, q.OptionB, q.OptionC, q.OptionD})
                                                        .ToListAsync();
            ViewBag.QuestionList = questionList;
            ViewBag.Time = test.Time;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Submit(int testId, List<SubmitDTO> submitDTOs, int userId )
        {
            var user = await _userManager.GetUserAsync(User);
            var test = await _context.Tests.FirstOrDefaultAsync(t => t.TestId == testId);
            if(test == null || !test.IsOpen)
            {
                return BadRequest("Cannot submit now");
            }
            if(user == null || user.Id != userId)
            {
                return Forbid();
            }
            var result = await _context.Results.FirstOrDefaultAsync(r => r.TestId == testId && r.UserId ==  userId);
            if(result == null)
            {
                return BadRequest("Student didn't enroll this exam");
            }
            List<SubmitHistory> submitHistories = new();
            try
            {
                foreach (var submitDTO in submitDTOs)
                {
                    var question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionsId == submitDTO.questionId) ?? throw new Exception("Cannot find question with id: " +  submitDTO.questionId);
                    SubmitHistory submitHistory = new SubmitHistory()
                    {
                        UserId = user.Id,
                        QuestionId = submitDTO.questionId,
                        answer = submitDTO.answer,
                        Question = question,
                    };
                    _context.Add(submitHistory);
                    submitHistories.Add(submitHistory);
                }
                result.SubmitAt = DateTime.Now;
                result.Score = CalcScore(testId, submitHistories);
                _context.Update(result);
                await _context.SaveChangesAsync();
            } catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Something go wrong: " + ex.Message);
                return View(ex.Message);
            }
            return RedirectToAction(nameof(Index));
        }

        public float CalcScore(int testId, List<SubmitHistory> submitHistories)
        {
            int count = 0;
            var test =  _context.Tests.FirstOrDefault(t => t.TestId == testId);
            if (test == null || submitHistories.Any(t => t.Question!.TestId != testId))
            {
                return -1;
            }
            foreach (var submitHistory in submitHistories)
            {
                if(submitHistory.answer.Equals(submitHistory.Question!.Answer)) { count++; }
            }
            return count/test.NumberOfQuestion;
        }
        private bool TestExists(int id)
        {
            return _context.Tests.Any(e => e.TestId == id);
        }


    }
}
