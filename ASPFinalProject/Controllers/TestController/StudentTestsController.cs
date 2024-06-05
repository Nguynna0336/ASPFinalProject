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

namespace ASPFinalProject.Controllers.TestController
{
    [Authorize]
    public class StudentTestsController : Controller
    {
        private readonly ExamDbContext _context;
        private readonly UserManager<User> _userManager;

        public StudentTestsController(ExamDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: StudentTests
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }
            var enrolledTest = _context.Results.Where(t => t.UserId == user.Id);
            var examDbContext = _context.Tests.Where(t => enrolledTest.Any(r => r.TestId == t.TestId));
            return View(await examDbContext.ToListAsync());
        }
        
        public async Task<IActionResult> Enroll()
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null)
            {
                return Challenge();
            }
            var enrolledTest = _context.Results.Where( t => t.UserId == user.Id);
            var unEnrolledTest = _context.Tests.Where(t => !enrolledTest.Any(r => r.TestId == t.TestId));
            return View(unEnrolledTest);
        }

        [HttpPost]
        public async Task<IActionResult> Enroll(int testId, int userId, string? password)
        {
            var user = await _userManager.GetUserAsync(User);
            var test = await _context.Tests.FirstOrDefaultAsync(t => t.TestId == testId);
            if(test == null) { 
                return NotFound(); 
            } else if(test.Password != null && test.Password.Equals(password))
            {
                ModelState.AddModelError(string.Empty, "Wrong password, please try again");
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
                Score = 0
            };
            try
            {
                _context.Add(result);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            } catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View();
            }
        }

        public async Task<IActionResult> Details(int testId)
        {
            var user = _userManager.GetUserAsync(User);
            if(user == null)
            {
                return Challenge();
            }
            var test = await _context.Tests.FirstOrDefaultAsync(t => t.TestId == testId);
            var result = await _context.Results.FirstOrDefaultAsync(t =>  t.TestId == testId);
            if(test == null || result == null)
            {
                return NotFound();
            }
            ViewBag.Test = test;
            if(!test.IsOpen)
            {
                ViewBag.Result = result;
            }
            return View();
        }
        public async Task<IActionResult> UnEnroll(int testId, int userId)
        {
            var user = await _userManager.GetUserAsync(User);
            if(user == null || user.Id != userId)
            {
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

        public async Task<IActionResult> StartTest(int testId)
        {
            var user = _userManager.GetUserAsync(User);
            var result = await _context.Results.FirstOrDefaultAsync(r => r.TestId == testId && r.UserId == user.Id);
            var test = await _context.Tests.FirstOrDefaultAsync(t => t.TestId == testId);
            if(test == null || !test.IsOpen)
            {
                return BadRequest("Test is not available now, try later");
            }
            if(result == null)
            {
                return BadRequest("You didnt enroll this test");
            }
            var questionList = await _context.Questions.Where(q => q.TestId == testId)
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
