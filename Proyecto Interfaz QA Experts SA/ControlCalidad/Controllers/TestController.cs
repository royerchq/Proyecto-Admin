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
    public class TestController : Controller
    {
        private QASystemEntities db = new QASystemEntities();

        // GET: Test
        public async Task<ActionResult> Index(int? id, int? projectId)
        {
            if (id != null && projectId != null) {

                RequirementController r = new RequirementController();

                ViewBag.requirementId = id;
                ViewBag.projectId = projectId;
                ViewBag.requirementName = r.getRequirementName(id);
                var pruebas = db.Pruebas.Include(p => p.Requerimiento).OrderByDescending(p => p.idPK);
                return View(await pruebas.ToListAsync());
            }

            return HttpNotFound();
            
        }

        // GET: Test/Details/5
        public async Task<ActionResult> Details(int? id, int? projectID, int? requirementID)
        {
            
            if (id == null || projectID == null || requirementID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Prueba prueba = await db.Pruebas.FindAsync( id,  projectID,  requirementID);
            if (prueba == null)
            {
                return HttpNotFound();
            }
            return View(prueba);
        }

        // GET: Test/Create
        public ActionResult Create(int? requirementId, int? projectId)
        {
            ViewBag.requirementId = requirementId;
            ViewBag.projectId = projectId;
            ViewBag.id_requerimientoFK = new SelectList(db.Requerimientoes, "idPK", "nombre");
            return View();
        }

        // POST: Test/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "idPK, id_proyectoFK, id_requerimientoFK, nombre, detalleResultado, resultadoFinal")] Prueba prueba)
        {
            int? projectId = prueba.id_proyectoFK;
            int? requirementeId = prueba.id_requerimientoFK;
            if (ModelState.IsValid && projectId != null && requirementeId != null)
            {
                db.Pruebas.Add(prueba);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { id = requirementeId, projectId = projectId } );
            }
            
            ViewBag.id_requerimientoFK = new SelectList(db.Requerimientoes, "idPK", "nombre", prueba.id_requerimientoFK);
            return View(prueba);
        }

        // GET: Test/Edit/5
        public async Task<ActionResult> Edit(int? id, int? projectID, int? requirementID)
        {
            if (id == null || projectID == null || requirementID == null)
            {
                return HttpNotFound();
            }

            Prueba prueba = await db.Pruebas.FindAsync(id, projectID, requirementID);
            if (prueba == null)
            {
                return HttpNotFound();
            }
            ViewBag.id_requerimientoFK = new SelectList(db.Requerimientoes, "idPK", "nombre", prueba.id_requerimientoFK);
            return View(prueba);
        }

        // POST: Test/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "idPK, id_proyectoFK, id_requerimientoFK, nombre, detalleResultado, resultadoFinal")] Prueba prueba)
        {
            if (ModelState.IsValid)
            {
                db.Entry(prueba).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Details", new { id = prueba.idPK,  projectID = prueba.id_proyectoFK, requirementID = prueba.id_requerimientoFK} );
            }
            ViewBag.id_requerimientoFK = new SelectList(db.Requerimientoes, "idPK", "nombre", prueba.id_requerimientoFK);
            return View(prueba);
        }

        // GET: Test/Delete/5
        public async Task<ActionResult> Delete(int? id, int? projectID, int? requirementID)
        {
            if (id == null || projectID == null || requirementID == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Prueba prueba = await db.Pruebas.FindAsync( id,  projectID,  requirementID);
            if (prueba == null)
            {
                return HttpNotFound();
            }
            return View(prueba);
        }

        // POST: Test/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Prueba prueba = await db.Pruebas.FindAsync(id);
            db.Pruebas.Remove(prueba);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public ActionResult RemoveTest(int? id, int project, int requirement)
        {
            Prueba prueba = db.Pruebas.Find(id, project, requirement);
            db.Pruebas.Remove(prueba);
            db.SaveChanges();
            return RedirectToAction("Index", new { id = prueba.id_requerimientoFK, projectId = prueba.id_proyectoFK });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


        //DOCUMENTAR POR ROBERTO
        public bool isAssigned(int requirement)
        {
            var exist = db.Pruebas.Any( test => test.id_requerimientoFK == requirement );
            return exist;
        }
    }
}
