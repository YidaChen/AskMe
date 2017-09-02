using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AskMe.Models;
using AskMe.Models.QuestionsView;
using Microsoft.AspNet.Identity;

namespace AskMe.Controllers
{
    public class QuestionsController : Controller
    {
        private QAEntities db = new QAEntities();

        // GET: Questions
        public ActionResult Index()
        {
            //var questions = db.Questions.Include(q => q.Answers1).Include(q => q.AspNetUsers);
            //return View(questions.ToList());
            return View();
        }
        public ActionResult Search(string s)
        {
            string sql = "SELECT q.Id, q.Title, q.ViewCount, q.CreateTime, " +
                    "COUNT(DISTINCT a.Id) AS AnswerCount, COUNT(DISTINCT f.UserId) AS FavorCount " +
                    "FROM Questions AS q " +
                    "LEFT JOIN Answers AS a ON q.Id = a.QuestionId " +
                    "LEFT JOIN UserFavorQuestions AS f ON q.Id = f.QuestionId " +
                    $"WHERE q.Title LIKE '%{s}%' " +
                    "GROUP BY q.Id, q.CreateTime, q.Title, q.ViewCount " +
                    "ORDER BY q.CreateTime DESC";

            List<IndexQuestion> questions = db.Database.SqlQuery<IndexQuestion>(sql).ToList();

            foreach (var q in questions)
            {
                var sql1 = $"SELECT Name FROM Tags INNER JOIN QuestionTags ON Tags.Id = QuestionTags.TagId WHERE QuestionId = {q.Id}";
                q.Tags = db.Database.SqlQuery<string>(sql1).ToList();
            }
            return View(questions);
        }
        public ActionResult ListBy(string id)
        {
            string sql = ListBySQL(id);
            List<IndexQuestion> questions = db.Database.SqlQuery<IndexQuestion>(sql).ToList();

            foreach(var q in questions)
            {
                var sql1 = $"SELECT Name FROM Tags INNER JOIN QuestionTags ON Tags.Id = QuestionTags.TagId WHERE QuestionId = {q.Id}";
                q.Tags = db.Database.SqlQuery<string>(sql1).ToList();
            }

            return Json(questions);
        }
        private string ListBySQL(string id)
        {
            string sql = "";

            if (id == "ListByFavor")//先不做
                sql = "";
            else if (id == "ListByHot")
                sql = "SELECT q.Id, q.Title, q.ViewCount, q.CreateTime, " +
                    "COUNT(DISTINCT a.Id) AS AnswerCount, COUNT(DISTINCT f.UserId) AS FavorCount " +
                    "FROM Questions AS q " +
                    "LEFT JOIN Answers AS a ON q.Id = a.QuestionId " +
                    "LEFT JOIN UserFavorQuestions AS f ON q.Id = f.QuestionId " +
                    "GROUP BY q.Id, q.CreateTime, q.Title, q.ViewCount " +
                    "ORDER BY (q.ViewCount+COUNT(DISTINCT a.Id)*1000+COUNT(DISTINCT f.UserId)*500) DESC";//和ListByLatest這裡不同
            else if (id == "ListByLatest")
                sql = "SELECT q.Id, q.Title, q.ViewCount, q.CreateTime, " +
                    "COUNT(DISTINCT a.Id) AS AnswerCount, COUNT(DISTINCT f.UserId) AS FavorCount " +
                    "FROM Questions AS q " +
                    "LEFT JOIN Answers AS a ON q.Id = a.QuestionId " +
                    "LEFT JOIN UserFavorQuestions AS f ON q.Id = f.QuestionId " +
                    "GROUP BY q.Id, q.CreateTime, q.Title, q.ViewCount " +
                    "ORDER BY q.CreateTime DESC";
            else if (id == "ListByNoAnswer")
                sql = "SELECT q.Id, q.Title, q.ViewCount, q.CreateTime, " +
                    "COUNT(DISTINCT a.Id) AS AnswerCount, COUNT(DISTINCT f.UserId) AS FavorCount " +
                    "FROM Questions AS q " +
                    "LEFT JOIN Answers AS a ON q.Id = a.QuestionId " +
                    "LEFT JOIN UserFavorQuestions AS f ON q.Id = f.QuestionId " +
                    "GROUP BY q.Id, q.CreateTime, q.Title, q.ViewCount " +
                    "HAVING COUNT(DISTINCT a.Id) = 0" +//ListByLatest的sql多加這行
                    "ORDER BY q.CreateTime DESC";

            return sql;
        }
        // GET: Questions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Questions questions = db.Questions.Find(id);
            if (questions == null)
            {
                return HttpNotFound();
            }
            questions.ViewCount++;
            db.Entry(questions).State = EntityState.Modified;
            db.SaveChanges();
            return View(questions);
        }

        // GET: Questions/Create
        [Authorize]
        public ActionResult Create()
        {
            //ViewBag.BestAnswerId = new SelectList(db.Answers, "Id", "UserId");
            //ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email");
            return View();
        }

        // POST: Questions/Create
        // 若要免於過量張貼攻擊，請啟用想要繫結的特定屬性，如需
        // 詳細資訊，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateViewModel formData)
        {
            if (ModelState.IsValid)
            {
                var q = new Questions();
                q.UserId = User.Identity.GetUserId();
                q.Title = formData.Title;
                q.Detail = formData.Detail.Replace("\r\n", "<br />");
                q.CreateTime = q.UpdateTime = DateTime.Now;

                foreach (string tagStr in formData.Tags)
                {   //判斷tag是否已存在, true表示id, false表示新的tag
                    int id;
                    if (int.TryParse(tagStr, out id))
                    {
                        Tags tag = db.Tags.FirstOrDefault(t => t.Id == id);
                        q.Tags.Add(tag);
                    } else
                    {
                        q.Tags.Add(new Tags { Name = tagStr });
                    }
                }
                db.Questions.Add(q);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            //ViewBag.BestAnswerId = new SelectList(db.Answers, "Id", "UserId", questions.BestAnswerId);
            //ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email", questions.UserId);
            return View(formData);
        }

        // GET: Questions/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Questions questions = db.Questions.Find(id);
            if (questions == null)
            {
                return HttpNotFound();
            }
            if (questions.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var formData = new EditViewModel()
            { Title = questions.Title, Detail = questions.Detail, Id = questions.Id};
            ViewBag.Tags = questions.Tags;
            return View(formData);
        }

        // POST: Questions/Edit/5
        // 若要免於過量張貼攻擊，請啟用想要繫結的特定屬性，如需
        // 詳細資訊，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditViewModel formData)
        {
            Questions q = db.Questions.SingleOrDefault(x => x.Id == formData.Id);
            if (ModelState.IsValid)
            {
                if (q == null)
                    return HttpNotFound();

                q.Title = formData.Title;
                q.Detail = formData.Detail.Replace("\r\n", "<br />");
                q.UpdateTime = DateTime.Now;
                q.Tags.Clear();
                foreach (string tagStr in formData.Tags)
                {   //判斷tag是否已存在, true表示id, false表示新的tag
                    int id;
                    if (int.TryParse(tagStr, out id))
                    {
                        Tags tag = db.Tags.FirstOrDefault(t => t.Id == id);
                        q.Tags.Add(tag);
                    }
                    else
                    {
                        q.Tags.Add(new Tags { Name = tagStr });
                    }
                }
                db.Entry(q).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { Id = formData.Id });
            }
            //ViewBag.BestAnswerId = new SelectList(db.Answers, "Id", "UserId", questions.BestAnswerId);
            //ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email", questions.UserId);
            ViewBag.Tags = q.Tags;
            return View(formData);
        }

        // GET: Questions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Questions questions = db.Questions.Find(id);
            if (questions == null)
            {
                return HttpNotFound();
            }
            return View(questions);
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Questions questions = db.Questions.Find(id);
            db.Questions.Remove(questions);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public ActionResult ChooseBestAnswer(int questionId, int answerId)
        {
            Questions q = db.Questions.SingleOrDefault(x => x.Id == questionId);
            if (q == null)
                return HttpNotFound();

            q.BestAnswerId = answerId;
            db.Entry(q).State = EntityState.Modified;
            db.SaveChanges();

            return Json("ok");
        }
        public ActionResult Tagged(string id)
        {
            var tag = db.Tags.FirstOrDefault(x => x.Name == id);

            if (tag == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            bool isFavoredTag = IsFavoredTag(tag.Id, User.Identity.GetUserId());

            ViewBag.isFavoredTag = isFavoredTag;
            return View(tag);
        }
        //用戶是否關注此標籤, 測試raw sql用法
        private bool IsFavoredTag(int tagId, string userId)
        {
            string sql = $"SELECT COUNT(*) FROM UserFavorTags WHERE UserId='{userId}' AND TagId={tagId}";
            int count = db.Database.SqlQuery<int>(sql).First();

            if (count > 0)
                return true;

            return false;
        }
        public ActionResult IsUserFavorQuestion(int questionId)
        {
            string userId = User.Identity.GetUserId();

            string sql = $"SELECT COUNT(*) FROM UserFavorQuestions WHERE UserId = '{userId}' AND QuestionId = {questionId}";
            int count = db.Database.SqlQuery<int>(sql).First();

            if (count > 0) return Json(true);

            return Json(false);
        }
        public ActionResult UserFavorQuestion(int questionId, string act)
        {
            string userId = User.Identity.GetUserId();
            string sql = $"DELETE FROM UserFavorQuestions WHERE UserId = '{userId}' AND QuestionId = {questionId}";
            var result = db.Database.ExecuteSqlCommand(sql);

            if (act == "favor")
            {
                string sql1 = $"INSERT INTO UserFavorQuestions (UserId, QuestionId) VALUES ('{userId}', {questionId})";
                var result1 = db.Database.ExecuteSqlCommand(sql1);
            }
            string sql2 = $"SELECT COUNT(*) FROM UserFavorQuestions WHERE QuestionId = {questionId}";
            int starCount = db.Database.SqlQuery<int>(sql2).First();

            return Json(starCount);
        }
        public ActionResult UserRateQuestion(int questionId, string act)
        {
            string userId = User.Identity.GetUserId();
            string sql = $"DELETE FROM UserRateQuestions WHERE UserId = '{userId}' AND QuestionId = {questionId}";
            var result = db.Database.ExecuteSqlCommand(sql);
            int IsPositive = 0;
            if (act == "Like") IsPositive = 1;

            string sql1 = $"INSERT INTO UserRateQuestions (UserId, QuestionId, IsPositive) VALUES ('{userId}', {questionId}, {IsPositive})";
            var result1 = db.Database.ExecuteSqlCommand(sql1);

            string sql2 = $"SELECT COUNT(*) FROM UserRateQuestions WHERE QuestionId = {questionId} AND IsPositive = 1";
            int trueCount = db.Database.SqlQuery<int>(sql2).First();

            string sql3 = $"SELECT COUNT(*) FROM UserRateQuestions WHERE QuestionId = {questionId} AND IsPositive = 0";
            int falseCount = db.Database.SqlQuery<int>(sql3).First();

            return Json(new { trueCount, falseCount});
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
