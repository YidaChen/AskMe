using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AskMe.Models;

namespace AskMe.Controllers
{
    public class QuestionsController : Controller
    {
        private QAEntities db = new QAEntities();

        // GET: Questions
        public ActionResult Index()
        {
            var questions = db.Questions.Include(q => q.Answers1).Include(q => q.AspNetUsers);
            return View(questions.ToList());
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
            return View(questions);
        }

        // GET: Questions/Create
        public ActionResult Create()
        {
            ViewBag.BestAnswerId = new SelectList(db.Answers, "Id", "UserId");
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email");
            return View();
        }

        // POST: Questions/Create
        // 若要免於過量張貼攻擊，請啟用想要繫結的特定屬性，如需
        // 詳細資訊，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,UserId,BestAnswerId,Title,Detail,ViewCount,IsClosed,CreateTime,UpdateTime,ClosedTime")] Questions questions)
        {
            if (ModelState.IsValid)
            {
                db.Questions.Add(questions);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.BestAnswerId = new SelectList(db.Answers, "Id", "UserId", questions.BestAnswerId);
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email", questions.UserId);
            return View(questions);
        }

        // GET: Questions/Edit/5
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
            ViewBag.BestAnswerId = new SelectList(db.Answers, "Id", "UserId", questions.BestAnswerId);
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email", questions.UserId);
            return View(questions);
        }

        // POST: Questions/Edit/5
        // 若要免於過量張貼攻擊，請啟用想要繫結的特定屬性，如需
        // 詳細資訊，請參閱 https://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,UserId,BestAnswerId,Title,Detail,ViewCount,IsClosed,CreateTime,UpdateTime,ClosedTime")] Questions questions)
        {
            if (ModelState.IsValid)
            {
                db.Entry(questions).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BestAnswerId = new SelectList(db.Answers, "Id", "UserId", questions.BestAnswerId);
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "Email", questions.UserId);
            return View(questions);
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
