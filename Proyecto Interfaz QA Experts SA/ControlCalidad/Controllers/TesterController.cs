using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ControlCalidad.Models;

namespace ControlCalidad.Controllers
{
    public class TesterController : Controller
    {
        private QASystemEntities db = new QASystemEntities();

        // GET: Tester
        public async Task<ActionResult> Index()
        {
            var testers = db.Testers.Include(t => t.Empleado);
            return View(await testers.ToListAsync());
        }

        // GET: Tester/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tester tester = await db.Testers.FindAsync(id);
            if (tester == null)
            {
                return HttpNotFound();
            }
            return View(tester);
        }

        // GET: Tester/Create
        public ActionResult Create()
        {
            ViewBag.cedula_empleadoFK = new SelectList(db.Empleadoes, "cedulaPK", "nombreP");
            return View();
        }

        // POST: Tester/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "cedula_empleadoFK,cantidadReqAsignados")] Tester tester)
        {
            if (ModelState.IsValid)
            {
                db.Testers.Add(tester);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.cedula_empleadoFK = new SelectList(db.Empleadoes, "cedulaPK", "nombreP", tester.cedula_empleadoFK);
            return View(tester);
        }

        // GET: Tester/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tester tester = await db.Testers.FindAsync(id);
            if (tester == null)
            {
                return HttpNotFound();
            }
            ViewBag.cedula_empleadoFK = new SelectList(db.Empleadoes, "cedulaPK", "nombreP", tester.cedula_empleadoFK);
            return View(tester);
        }

        // POST: Tester/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "cedula_empleadoFK,cantidadReqAsignados")] Tester tester)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tester).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.cedula_empleadoFK = new SelectList(db.Empleadoes, "cedulaPK", "nombreP", tester.cedula_empleadoFK);
            return View(tester);
        }

        // GET: Tester/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tester tester = await db.Testers.FindAsync(id);
            if (tester == null)
            {
                return HttpNotFound();
            }
            return View(tester);
        }

        // POST: Tester/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Tester tester = await db.Testers.FindAsync(id);
            db.Testers.Remove(tester);
            await db.SaveChangesAsync();
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

        //<summary> : gets all testers from database to put them in a list
        //<param>   : None
        //<return>  : List<SelectListItem>, a testers list
        public List<SelectListItem> getAllTesters()
        {
            List<USP_obtenerTestersExistentes_Result> testers = db.USP_obtenerTestersExistentes( ).ToList( );
            List<SelectListItem> allTesters = testers.ConvertAll(
                tester => {
                    return new SelectListItem( ) {
                        Text = tester.nombreTester ,
                        Value = tester.cedula ,
                        Selected = false
                    };
                } );
            return allTesters;
        }
    }
}
