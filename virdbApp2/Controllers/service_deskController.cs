using PagedList; // vovkor
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using virdbApp2.Models;
using Microsoft.AspNet.Identity;
// vovkor для RequiredLabel, чтобы обязательные роля были со звездочками  НАЧАЛО
using System.Linq.Expressions;
using System.ComponentModel;

namespace virdbApp2.Controllers
{
    public class service_deskController : Controller
    {
        private virdb2Entities db = new virdb2Entities();

        // GET: service_desk
        public ActionResult Index()
        {
            var service_desk = db.service_desk.Include(s => s.sd_kind).Include(s => s.sd_status).Include(s => s.user);

            // Даём Андрею редактировать все заявки
            if ( User.Identity.GetUserName().Trim().ToUpper() == "VIKTOR202")
            {ViewBag.hasPermission = "1";}
            else
            { ViewBag.hasPermission = "0"; }
            
            return View(service_desk.ToList());
        }

        // GET: service_desk/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            service_desk service_desk = db.service_desk.Find(id);
            if (service_desk == null)
            {
                return HttpNotFound();
            }
            return View(service_desk);
        }

        // GET: service_desk/Create
        public ActionResult Create()
        {
            ViewBag.kind_id = new SelectList(db.sd_kind, "id", "name");
            ViewBag.status_id = new SelectList(db.sd_status, "id", "name");
            ViewBag.user_id = new SelectList(db.users, "username", "fio");
            //ViewBag.date_begin = DateTime.Now;
            return View();
        }

        // POST: service_desk/Create
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "number_request,author,kind_id,date_begin,date_end,status_id,comment,task,what_done,user_id,fio")] service_desk service_desk)

        {
            if (ModelState.IsValid)
            {
                service_desk.author = User.Identity.GetUserName();
                service_desk.date_begin = DateTime.Now;
                service_desk.status_id = 1;
                db.service_desk.Add(service_desk);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.kind_id = new SelectList(db.sd_kind, "id", "name", service_desk.kind_id);
            ViewBag.status_id = new SelectList(db.sd_status, "id", "name", service_desk.status_id);
            ViewBag.user_id = new SelectList(db.users, "username", "fio", service_desk.user_id);
            return View(service_desk);
        }

        // GET: service_desk/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            service_desk service_desk = db.service_desk.Find(id);
            if (service_desk == null)
            {
                return HttpNotFound();
            }
            ViewBag.kind_id = new SelectList(db.sd_kind, "id", "name", service_desk.kind_id);
            ViewBag.status_id = new SelectList(db.sd_status, "id", "name", service_desk.status_id);
            ViewBag.user_id = new SelectList(db.users, "username", "fio", service_desk.user_id);
            // для передачи в представление используем ViewBag
            // Если заявка закрыта, то статус не показываем
            if (service_desk.status_id == 1)
            {ViewBag.showStatus = true;}
            else
            { ViewBag.showStatus = false;}
             


            return View(service_desk);
        }

        // POST: service_desk/Edit/5
        // Чтобы защититься от атак чрезмерной передачи данных, включите определенные свойства, для которых следует установить привязку. Дополнительные 
        // сведения см. в статье http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "number_request,author,kind_id,date_begin,date_end,status_id,comment,task,what_done,user_id,fio")] service_desk service_desk)
        {
            if (ModelState.IsValid)
            {
                if (service_desk.status_id == 2 && service_desk.date_end == null )
                {
                    service_desk.date_end = DateTime.Now;
                }
 
                db.Entry(service_desk).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.kind_id = new SelectList(db.sd_kind, "id", "name", service_desk.kind_id);
            ViewBag.status_id = new SelectList(db.sd_status, "id", "name", service_desk.status_id);
            ViewBag.user_id = new SelectList(db.users, "username", "fio", service_desk.user_id);
            return View(service_desk);
        }

        // GET: service_desk/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            service_desk service_desk = db.service_desk.Find(id);
            if (service_desk == null)
            {
                return HttpNotFound();
            }
            return View(service_desk);
        }

        // POST: service_desk/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            service_desk service_desk = db.service_desk.Find(id);
            db.service_desk.Remove(service_desk);
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
