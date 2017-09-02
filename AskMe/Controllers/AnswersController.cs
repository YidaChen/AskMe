using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AskMe.Models;
using Microsoft.AspNet.Identity;

namespace AskMe.Controllers
{
    public class AnswersController : Controller
    {
        private QAEntities db = new QAEntities();

        // GET: Answers
        public ActionResult Index()
        {
            var answers = db.Answers.Include(a => a.AspNetUsers).Include(a => a.Questions);
            return View(answers.ToList());
        }

        // GET: Answers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Answers answers = db.Answers.Find(id);
            if (answers == null)
            {
                return HttpNotFound();
            }
            return View(answers);
        }

        // GET: Answers/Create
        public ActionResult Create()
        {
            //ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email");
            //ViewBag.QuestionId = new SelectList(db.Questions, "Id", "UserId");
            return View();
        }

        // POST: Answers/Create
        // 若要免於過量張貼攻擊，請啟用想要繫結的特定屬性，如需
        // 詳細資訊，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(int QuestionId, string Detail)
        {
            Answers ans = new Answers() { UserId = User.Identity.GetUserId(),
                QuestionId = QuestionId,
                Detail = Detail.Replace("\r\n", "<br />"),
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now
            };
            db.Answers.Add(ans);
            db.SaveChanges();

            return RedirectToAction("Details", "Questions", new { id = QuestionId });
        }

        // GET: Answers/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Answers answers = db.Answers.Find(id);
            if (answers == null)
            {
                return HttpNotFound();
            }
            if(answers.UserId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            return View(answers);
        }

        // POST: Answers/Edit/5
        // 若要免於過量張貼攻擊，請啟用想要繫結的特定屬性，如需
        // 詳細資訊，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string Detail, int id)
        {
            Answers ans = db.Answers.Find(id);

            if (ans.UserId != User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (Detail != "")
            {
                ans.UpdateTime = DateTime.Now;
                ans.Detail = Detail.Replace("\r\n", "<br />"); ;
                db.Entry(ans).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", "Questions", new { id = ans.QuestionId });
            }
            return View(ans);
        }
        public ActionResult UserRateAnswer(int answerId, string act)
        {
            string userId = User.Identity.GetUserId();
            string sql = $"DELETE FROM UserRateAnswers WHERE UserId = '{userId}' AND AnswerId = {answerId}";
            var result = db.Database.ExecuteSqlCommand(sql);
            int IsPositive = 0;
            if (act == "Like") IsPositive = 1;

            string sql1 = $"INSERT INTO UserRateAnswers (UserId, AnswerId, IsPositive) VALUES ('{userId}', {answerId}, {IsPositive})";
            var result1 = db.Database.ExecuteSqlCommand(sql1);

            string sql2 = $"SELECT COUNT(*) FROM UserRateAnswers WHERE AnswerId = {answerId} AND IsPositive = 1";
            int trueCount = db.Database.SqlQuery<int>(sql2).First();

            string sql3 = $"SELECT COUNT(*) FROM UserRateAnswers WHERE AnswerId = {answerId} AND IsPositive = 0";
            int falseCount = db.Database.SqlQuery<int>(sql3).First();

            return Json(new { trueCount, falseCount });
        }
        // GET: Answers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Answers answers = db.Answers.Find(id);
            if (answers == null)
            {
                return HttpNotFound();
            }
            return View(answers);
        }

        // POST: Answers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Answers answers = db.Answers.Find(id);
            db.Answers.Remove(answers);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
