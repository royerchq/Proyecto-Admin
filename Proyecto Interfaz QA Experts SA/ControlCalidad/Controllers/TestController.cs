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
            
                return View();
           
            
        }

        // GET: Test/Details/5
        public ActionResult Details(int? id, int? projectID, int? requirementID)
        {
            return View();
        }

        // GET: Test/Create
        public ActionResult Create(int? requirementId, int? projectId)
        {
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
        public ActionResult Edit(int? id, int? projectID, int? requirementID)
        {
           
            return View();
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
        public ActionResult Delete(int? id, int? projectID, int? requirementID)
        {
            return View();
        }

        // POST: Test/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            return RedirectToAction("Index");
        }

        public ActionResult RemoveTest(int? id, int project, int requirement)
        {
            return RedirectToAction("Index", new { id = 1, projectId = 1 });
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
