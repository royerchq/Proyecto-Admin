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
using System.Text.RegularExpressions;

namespace ControlCalidad.Controllers
{
    public class EmployeeController : Controller
    {
        private QASystemEntities db = new QASystemEntities();
        private localizationsController localizations = new localizationsController();
        private static string editID;
        private string projectLeader;


        // GET: Employee
        public async Task<ActionResult> Index()
        {
            var empleadoes = db.Empleadoes.Include(e => e.Tester);
           
            return View(await empleadoes.ToListAsync());
        }

        // GET: Employee/Details/5
        public async Task<ActionResult> Details(string id)
        {

            db.USP_calcularEdadEmpleado( id );
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Empleado empleado = await db.Empleadoes.FindAsync(id);
            DateTime dateOfBirth = (DateTime)empleado.fechaNacimiento;
            ViewBag.dateOfBirth = dateOfBirth.ToShortDateString();
            if (empleado == null)
            {
                return HttpNotFound();
            }
            return View(empleado);
        }

        // GET: Employee/Create
        public ActionResult Create()
        {
            ViewBag.provinces = this.localizations.provinceList();
            ViewBag.cedulaPK = new SelectList(db.Testers, "cedula_empleadoFk", "cedula_empleadoFk");
            return View();
        }

        // POST: Employee/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "cedulaPK,nombreP,apellido1,apellido2,fechaNacimiento,edad,telefono,correo,provincia,canton,distrito,direccionExacta,disponibilidad")] Empleado empleado)
        {
            string provinceName = localizations.provinceName(empleado.provincia);
            string cantonName = localizations.cantonName(empleado.provincia, empleado.canton);
            string districtName = localizations.districtName(empleado.provincia, empleado.canton, empleado.distrito);
            empleado.provincia = provinceName;
            empleado.canton = cantonName;
            empleado.distrito = districtName;
            if (ModelState.IsValid)
            {
                db.Empleadoes.Add(empleado);
                try
                {
                    await db.SaveChangesAsync( );
                }
                catch
                {
                    ModelState.AddModelError( "" , "No puedo agregar usuarios con la misma cédula." );
                    return View( empleado );
                }
               
                return RedirectToAction("../Habilities/Index",new {cedula_empleado = empleado.cedulaPK });
            }

            ViewBag.cedulaPK = new SelectList(db.Testers, "cedula_empleadoFk", "cedula_empleadoFk", empleado.cedulaPK);
            return View(empleado);
        }

        // GET: Employee/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            ViewBag.provinces = this.localizations.provinceList();
            editID = string.Copy(id);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Empleado empleado = await db.Empleadoes.FindAsync(id);
            if (empleado == null)
            {
                return HttpNotFound();
            }
            ViewBag.cedulaPK = new SelectList(db.Testers, "cedula_empleadoFk", "cedula_empleadoFk", empleado.cedulaPK);
            return View(empleado);
        }

        // POST: Employee/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "cedulaPK,nombreP,apellido1,apellido2,fechaNacimiento,edad,telefono,correo,provincia,canton,distrito,direccionExacta,disponibilidad")] Empleado empleado)
        {
            var editProvince = "";
            var editCanton = "";
            var editDistrict ="";

            if(empleado.provincia != null)
            {
                editProvince = Regex.Match(empleado.provincia, @"\d+").Value;
            }
            if (empleado.provincia != null)
            {
                editCanton = Regex.Match(empleado.canton, @"\d+").Value;
            }
            if (empleado.provincia != null)
            {
                editDistrict = Regex.Match(empleado.distrito, @"\d+").Value;
            }

            string provinceName = null;
            string cantonName = null;
            string districtName = null;
            string provinceID = null;

            if (editProvince != "")
            {
                provinceName = localizations.provinceName(empleado.provincia);
                empleado.provincia = provinceName;

            }
            if (editCanton != "")
            {
                provinceID = this.localizations.provinceID(provinceName).ToString();
                cantonName = localizations.cantonName(provinceID, empleado.canton);
                empleado.canton = cantonName;

            }
            if (editDistrict != "")
            {
                string cantonID = this.localizations.cantonID(provinceName,empleado.canton).ToString();
                districtName = localizations.districtName(provinceID, cantonID, empleado.distrito);
                empleado.distrito = districtName;
            }


            if (ModelState.IsValid)
            {

                db.Edit_Empleado(editID, empleado.cedulaPK, empleado.nombreP, empleado.apellido1, empleado.apellido2,
                                 empleado.fechaNacimiento, empleado.edad, empleado.telefono, empleado.correo, empleado.provincia,
                                 empleado.canton, empleado.distrito, empleado.direccionExacta, empleado.disponibilidad);

                return RedirectToAction("Edit", new { id = empleado.cedulaPK });
            }
            ViewBag.cedulaPK = new SelectList(db.Testers, "cedula_empleadoFk", "cedula_empleadoFk", empleado.cedulaPK);
            return View(empleado);
        }

        // GET: Employee/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Empleado empleado = await db.Empleadoes.FindAsync(id);
            if (empleado == null)
            {
                return HttpNotFound();
            }
            return View(empleado);
        }

        // POST: Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Empleado empleado = await db.Empleadoes.FindAsync(id);
            db.Empleadoes.Remove(empleado);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public ActionResult RemoveEmployee(string employeeId)
        {
            Empleado employee = db.Empleadoes.Find(employeeId);
            db.Empleadoes.Remove(employee);
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

        //<summary> : This method is used to know what employees are available to be a leader of a project.
        //<params>  : None
        //<return>  : Returns a list of type "SelectListItem" that contains values of the employees that are available to be a leader.
        public List<SelectListItem> GetLeaders()
        {
            string query = "SELECT	E.nombreP, E.apellido1, E.apellido2, E.cedulaPK, E.disponibilidad FROM ControlCalidad.Empleado E WHERE E.cedulaPK NOT IN(SELECT  T.cedula_empleadoFk FROM    ControlCalidad.Tester T) " +
                "AND E.disponibilidad = 'Disponible';";
            List<LeaderForProject> leaderList = db.Database.SqlQuery<LeaderForProject>(query).ToList();

            foreach (LeaderForProject leader in leaderList){
                leader.nombreCompleto = leader.nombreP + " " + leader.apellido1 + " " + leader.apellido2;
            }

            List< SelectListItem > leadersItemList = leaderList.ConvertAll(
                leader => {
                    return new SelectListItem()
                    {
                        Text = leader.nombreCompleto,
                        Value = leader.cedulaPK.ToString(),
                        Selected = false
                    };
                });
            return leadersItemList;
        }


        //<summary> : gets employees from database to put them in a list
        //<param>   : None
        //<return>  : List<SelectListItem>, a employees list
        public List<SelectListItem> GetTesters()
        {
            string query = "SELECT	E.nombreP FROM ControlCalidad.Empleado E WHERE E.cedulaPK IN(SELECT T.cedula_empleadoFk FROM ControlCalidad.Tester T) ";
            List<EmployeeForReports> testerList = db.Database.SqlQuery<EmployeeForReports>(query).ToList();

            List<SelectListItem> alltesters = testerList.ConvertAll(
                tester => {
                    return new SelectListItem()
                    {
                        Text = tester.nombreP,
                    };
                });
            return alltesters;
        }

        //<summary> : gets employees from database to put them in a list
        //<param>   : None
        //<return>  : List<SelectListItem>, a employees list
        public List<SelectListItem> GetNameTesters()
        {
            string query = "SELECT	E.nombreP FROM ControlCalidad.Empleado E WHERE E.cedulaPK IN(SELECT T.cedula_empleadoFk FROM ControlCalidad.Tester T) ";
            List<EmployeeForReportsT> testerList = db.Database.SqlQuery<EmployeeForReportsT>(query).ToList();

            List<SelectListItem> allNametesters = testerList.ConvertAll(
                tester => {
                    return new SelectListItem()
                    {
                        Text = tester.nombreP,
                    };
                });
            return allNametesters;
        }

        //<summary> : This method is used to know the identifier of an employee just by passing his email.
        //<params>  : email : The email of the employee we want to know his id.
        //<return>  : Returns the identifier of an employee.
        public string GetEmployeeIdByEmail(string email)
        {
            string employeeId = "";
            if (email != null) {
                string query = "SELECT E.cedulaPK FROM ControlCalidad.Empleado E " +
                               "WHERE  E.correo = '" + email +"'";
                List<idEmpleado> employeeList = db.Database.SqlQuery<idEmpleado>(query).ToList();
                var employee = employeeList.Last();
                employeeId = employee.cedulaPk;
            }
            
            return employeeId;
        }

        //<summary> : This method is used to know the name of an employee just by passing his id.
        //<params>  : id : Identifier of an employee we want to know his name.
        //<return>  : Returns the name of an employee.
        public string employeeName(string id)
        {
            List<Empleado> empleado = db.Empleadoes.Where(x => x.cedulaPK == id).ToList();
            return empleado[0].nombreP;
        }

        //<summary> : This method is used to know if one email has been taken from another employee or client.
        //<params>  : input : It's the email we want to validate if is taken or not.
        //<return>  : Returns a boolean value, true if the email was taken, false the otherwise.
        public bool isMailTaken(string input)
        {
            List<Empleado> empleado = db.Empleadoes.Where(x => x.correo == input).ToList();
            if(empleado != null)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        //<summary> : This method is used to verify if an ID has already been taken from another employee.
        //<params>  : input : It's the ID we want to validate if is taken or not.
        //<return>  : Returns a boolean value, true if the ID was registered, false the otherwise.
        public bool existID(string id)
        {
            List<Empleado> empleado = db.Empleadoes.Where(x => x.cedulaPK == id).ToList();
            if(empleado.Count >= 1)
            {
                return true;
            }

            return false;

        }

    }
}
