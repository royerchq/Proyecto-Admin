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
    // <class> : Class designed to receive the results of the queries of employee table
    class DbResultE
    {
        public string cedulaPK { get; set; }
        public string nombreP { get; set; }
    }

    public class TeamController : Controller
    {
        private QASystemEntities db = new QASystemEntities();
        public string[] badCommands = { "--", "insert", "drop", "update", "delete", "or", "and", "join",";", "" + '"', "%", "=" };
        public bool goodQuery(string ability) {
            for (int index = 0; index < badCommands.Length; ++index) {
                if (ability.ToLower().Contains(badCommands[index]))
                    return false;
            }
            return true;
        }

        // GET: Team/Edit/5
        //<summary> :   View for teams, prepare several queries, to select the skills of an employee 
        //              in case a search is made by skills, also searches all available employees and 
        //              all employees linked to the specific project
        //<param>   :   int id_proyecto: represents the primary key of the project table, 
        //                               this to bring the team and the name of the project.
        //              string ability: represents the selected skills in the filter
        //<return>  : View of teams, with all data
        public ActionResult Edit(int id_proyecto, string ability)
        {
            //This vector works to accumulate all comma separated abilities and then perform the query
            string[] abilities = null;
            string sqlAbilities= "";
            if (ability != null ){
                abilities = ability.Split(',');
                string template = "H.descripcionPK LIKE '%";
                for (int index = 0; index < abilities.Length; ++index)
                {
                    if (index == 0) {
                        sqlAbilities = template + abilities[index].Trim() + "%'";
                    }
                    else
                    {
                        sqlAbilities += " AND " + template + abilities[index].Trim() + "%'";
                    }
                }
            }
            string sqlp = "SELECT nombre FROM ControlCalidad.Proyecto WHERE idPK=" + id_proyecto;
            string name = db.Database.SqlQuery<string>(sqlp).ToList()[0];
            ViewBag.project_name = name;
            string sql = "SELECT E.cedulaPK, E.nombreP+' '+E.apellido1+' '+E.apellido2 AS 'nombreP' FROM ControlCalidad.Empleado E JOIN ControlCalidad.TrabajaEn T ON T.cedula_empleadoFK = E.cedulaPK WHERE T.id_proyectoFK = " + id_proyecto;
            List<DbResultE> team = db.Database.SqlQuery<DbResultE>(sql).ToList();

            List<SP_Conseguir_CantReqs_Result> reqs_testers = db.SP_Conseguir_CantReqs(id_proyecto).ToList();
            List<SelectListItem> all_cant_reqs = reqs_testers.ConvertAll(
                tester => {
                    return new SelectListItem()
                    {
                        Text = tester.cantidadReqAsignados.ToString(),
                        Value = tester.cedulaPK,
                        Selected = false
                    };
                });
            ViewBag.cant_reqs = all_cant_reqs;
            ViewBag.cedula_empleadoFK = new SelectList(team, "cedulaPK", "nombreP", "Equipo");
            string sqle = "SELECT E.cedulaPK , E.nombreP+' '+E.apellido1+' '+E.apellido2 AS 'nombreP' " +
                "FROM ControlCalidad.Empleado E JOIN ControlCalidad.Habilidades H ON H.cedula_empleadoFK = E.cedulaPK " +
                "WHERE "+ sqlAbilities +
                " AND E.disponibilidad = 'Disponible'";
            if (ability == null)
            {
                sqle = " SELECT E.cedulaPK , E.nombreP+' '+E.apellido1+' '+E.apellido2 AS 'nombreP' FROM ControlCalidad.Empleado E WHERE E.disponibilidad = 'Disponible'";
            }
            else {
                if (!goodQuery(ability))
                {
                    sqle = " SELECT E.cedulaPK , E.nombreP+' '+E.apellido1+' '+E.apellido2 AS 'nombreP' FROM ControlCalidad.Empleado E WHERE E.disponibilidad = 'Disponible'";
                }
            }
            List<DbResultE> all_available = db.Database.SqlQuery<DbResultE>(sqle).ToList();
            List<SelectListItem> all_availables = all_available.ConvertAll(
                tester => {
                    return new SelectListItem()
                    {
                        Text = tester.nombreP,
                        Value = tester.cedulaPK,
                        Selected = false
                    };
                });
            List<SelectListItem> all_availables_temp = new List<SelectListItem>();
            bool exists = false;
            foreach (SelectListItem available in all_availables) {
                foreach (SelectListItem team_employee in ViewBag.cedula_empleadoFK)
                {
                    if (available.Value == team_employee.Value) {
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                {
                    all_availables_temp.Add(available);
                }
                exists = false;
            }
            ViewBag.disponibles = all_availables_temp;
            ViewBag.id_proyecto = id_proyecto;
            return View();
        }

        //<summary> :   Class that reacts by submit button, it extracts all the necessary data in case of assignment, 
        //              deallocation or search
        //<param>   :   FormCollection fc: represents all view forms
        //              string allocate: represents the action of allocate
        //              string deallocate: represents the action of deallocate
        //              string search: represents the action of search
        //<return>  : Redirect to viwe of teams, with all new data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(FormCollection fc, string allocate, string deallocate, string search)
        {
            if (search != null)
            {
                string skill = fc["Ability"];
                return RedirectToAction("Edit", new { id_proyecto = Convert.ToInt32(fc["Project"]), ability = skill });
            }
            string cedulaPK = "";
            int id_proyecto = -1;
            string sql = "";
            int result = -1;
            if (allocate != null)
            {
                cedulaPK = fc["IdNew"];
                id_proyecto = Convert.ToInt32(fc["Project"]);
                string sqls = "SELECT E.cedulaPK, E.nombreP+' '+E.apellido1+' '+E.apellido2 AS 'nombreP' FROM ControlCalidad.Empleado E JOIN ControlCalidad.TrabajaEn T ON T.cedula_empleadoFK = E.cedulaPK WHERE T.id_proyectoFK = " + id_proyecto;
                List<DbResultE> team = db.Database.SqlQuery<DbResultE>(sqls).ToList();
                if (team.Count == 5)
                {
                    return RedirectToAction("Edit", new { id_proyecto = id_proyecto });
                }

                sql = "INSERT INTO ControlCalidad.TrabajaEn VALUES('" + cedulaPK + "'," + id_proyecto + ", 'Tester')";
                try
                {
                    result = db.Database.ExecuteSqlCommand(sql);
                    sql = "INSERT INTO ControlCalidad.Tester VALUES('" + cedulaPK + "', 0)";
                    result = db.Database.ExecuteSqlCommand(sql);
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                try
                {
                    sql = "UPDATE ControlCalidad.Empleado SET disponibilidad = 'Ocupado' WHERE cedulaPK = '" + cedulaPK + "'";
                    result = db.Database.ExecuteSqlCommand(sql);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
            }
            else
            {
                cedulaPK = fc["IdMember"];
                id_proyecto = Convert.ToInt32(fc["Project"]);
                sql = "DELETE FROM ControlCalidad.TrabajaEn WHERE cedula_empleadoFK = '" + cedulaPK + "' AND id_proyectoFK = " + id_proyecto + ";";
                try
                {
                    result = db.Database.ExecuteSqlCommand(sql);
                    sql = "DELETE FROM ControlCalidad.Tester WHERE cedula_empleadoFK = '" +cedulaPK + "';";
                    result = db.Database.ExecuteSqlCommand(sql);
                    sql = "UPDATE ControlCalidad.Empleado SET disponibilidad = 'Disponible' WHERE cedulaPK = '" + cedulaPK + "'";
                    result = db.Database.ExecuteSqlCommand(sql);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return RedirectToAction("Edit", new { id_proyecto = id_proyecto });
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