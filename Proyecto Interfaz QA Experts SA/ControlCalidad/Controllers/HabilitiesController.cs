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
    public class HabilitiesController : Controller
    {
        private QASystemEntities db = new QASystemEntities();
        private EmployeeController emp = new EmployeeController();
        private static string cedulaEdit;
        private static string categoriaEdit;
        private static string descripcionEdit;

        // GET: Habilities
        public async Task<ActionResult> Index(string empleado)
        {
            var habilidades = db.Habilidades.Include(h => h.Empleado);
            string id;

            if (string.IsNullOrEmpty(empleado))
            {
                string rawUrl = Request.RawUrl;
                string[] splitUrl = rawUrl.Split('=');

                try
                {
                    id = splitUrl[1];
                    ViewBag.empleado = splitUrl[1];
                    ViewBag.employeeName = this.emp.employeeName(id);
                }
                catch (Exception e)
                {
                    Console.Write("ignorar");
                }
            }
            else
            {
                ViewBag.empleado = empleado;
                ViewBag.employeeName = this.emp.employeeName(empleado);
            }
            return View(await habilidades.ToListAsync());
        }

        // GET: Habilities/Details/5
        public async Task<ActionResult> Details(string cedula_empleadoFK, string categoriaPK, string descripcionPK)
        {
            ViewBag.idDelete = cedula_empleadoFK;
            ViewBag.categoryID = categoriaPK;
            ViewBag.descriptionDelete = descripcionPK;
            if (cedula_empleadoFK == null)
            {
                return RedirectToAction("Index", new { cedula_empleadoFK = cedula_empleadoFK });
            }
            Habilidade habilidade = await db.Habilidades.FindAsync(cedula_empleadoFK, categoriaPK, descripcionPK);
            if (habilidade == null)
            {
                 return RedirectToAction("Index", new { cedula_empleadoFK = habilidade.cedula_empleadoFK });
            }
            return View(habilidade);
        }

        // GET: Habilities/Create
        public ActionResult Create(string cedulaEmpleado)
        {
            string id;
            if (string.IsNullOrEmpty(cedulaEmpleado))
            {
                string rawUrl = Request.RawUrl;
                string[] splitUrl = rawUrl.Split('=');

                try
                {
                    id = splitUrl[1];
                    ViewBag.cedulaCreate = splitUrl[1];
                    ViewBag.employeeName = this.emp.employeeName(id);
                }
                catch (Exception e)
                {
                    Console.Write("ignorar");
                }
            }

            ViewBag.cedula_empleadoFK = new SelectList(db.Empleadoes, "cedulaPK", "nombreP");
            return View();
        }

        // POST: Habilities/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "cedula_empleadoFK,categoriaPK,descripcionPK")] Habilidade habilidad)
        {
            if (ModelState.IsValid)
            {
                db.Habilidades.Add(habilidad);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", new { cedula_empleadoFK = habilidad.cedula_empleadoFK });
            }

            ViewBag.cedula_empleadoFK = new SelectList(db.Empleadoes, "cedulaPK", "nombreP", habilidad.cedula_empleadoFK);
            return View(habilidad);
        }

        // GET: Habilities/Edit/5
        public async Task<ActionResult> Edit(string cedula_empleadoFK, string categoriaPK, string descripcionPK)
        {
            cedulaEdit = string.Copy(cedula_empleadoFK);
            categoriaEdit = string.Copy(categoriaPK);
            descripcionEdit = string.Copy(descripcionPK);
            ViewBag.cedulaEdit = string.Copy(cedula_empleadoFK);
            ViewBag.categoriaEdit = string.Copy(categoriaPK);
            ViewBag.descripcionEdit = string.Copy(descripcionPK);

            if (cedula_empleadoFK == null)
            {
                return RedirectToAction("../Employee/Index");
            }
            Habilidade habilidade = await db.Habilidades.FindAsync(cedula_empleadoFK,categoriaPK,descripcionPK);
            if (habilidade == null)
            {
                return RedirectToAction("../Employee/Index");
            }
            ViewBag.cedula_empleadoFK = new SelectList(db.Empleadoes, "cedulaPK", "nombreP", habilidade.cedula_empleadoFK);
            return View(habilidade);
        }

        // POST: Habilities/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que desea enlazarse. Para obtener 
        // más información vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "cedula_empleadoFK,categoriaPK,descripcionPK")] Habilidade habilidad)
        {
            if (ModelState.IsValid)
            {
                var sql =
                    from a in db.Habilidades
                    where a.cedula_empleadoFK == cedulaEdit
                    && a.descripcionPK == descripcionEdit
                    && a.categoriaPK == categoriaEdit
                    select a;
                
                foreach (var a in sql)
                {
                    db.Habilidades.Remove(a);
                    break; 
                }

                try
                {
                   await db.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    // Provide for exceptions.
                }
                
                db.Habilidades.Add(habilidad);
                await db.SaveChangesAsync();

                return RedirectToAction("Index", new { cedula_empleadoFK = habilidad.cedula_empleadoFK });
            }
            ViewBag.cedula_empleadoFK = new SelectList(db.Empleadoes, "cedulaPK", "nombreP", habilidad.cedula_empleadoFK);
            return View(habilidad);
        }

        // GET: Habilities/Delete/5
        public async Task<ActionResult> Delete(string cedula_empleadoFK, string categoriaPK, string descripcionPK)
        {
            if (cedula_empleadoFK == null)
            {
                return RedirectToAction("../Employee/Index");
            }
            Habilidade habilidad = await db.Habilidades.FindAsync(cedula_empleadoFK,  categoriaPK,  descripcionPK);
            if (habilidad == null)
            {
                return RedirectToAction("../Employee/Index");
            }
            return View(habilidad);
        }

        // POST: Habilities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string cedula_empleadoFK, string categoriaPK, string descripcionPK)
        {
            Habilidade habilidade = await db.Habilidades.FindAsync(cedula_empleadoFK, categoriaPK, descripcionPK);
            db.Habilidades.Remove(habilidade);
            await db.SaveChangesAsync();
            return RedirectToAction("Index",new { cedula_empleadoFK = cedula_empleadoFK});
        }
        public ActionResult RemoveHability(string id,string category, string description)
        {
            Habilidade habilidad = db.Habilidades.Find(id,category,description);
            db.Habilidades.Remove(habilidad);
            db.SaveChanges();
            return RedirectToAction("Index", new { cedula_empleadoFK = habilidad.cedula_empleadoFK });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public List<SelectListItem> getHabilitiesCategories()
        {
            List<string> categoriesList = db.Habilidades.Select(category => category.categoriaPK).Distinct().ToList();


            List<SelectListItem> allCategories = categoriesList.ConvertAll(
                category => {
                    return new SelectListItem()
                    {
                        Text = category,
                        Value = category,
                        Selected = false
                    };
                });

            return allCategories;
        }

        public JsonResult getHabilitiesByCategory(string category) {

            db.Configuration.ProxyCreationEnabled = false;

            List<string> allHabilities = db.USP_obtenerHabilidadesPorCategoria(category).ToList<string>();

            return Json(allHabilities, JsonRequestBehavior.AllowGet);

        }
    }
}
