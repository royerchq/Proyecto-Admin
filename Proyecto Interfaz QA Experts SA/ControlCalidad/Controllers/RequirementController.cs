using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web.Mvc;
using ControlCalidad.Models;

namespace ControlCalidad.Controllers
{
    public class RequirementController : Controller
    {
        private QASystemEntities db = new QASystemEntities( );

        //Controller to get information from projects
        private ProjectController projectController = new ProjectController( );

        //Controller to get information about tester and their assigned requirements
        private TesterRequirementController tieneAsignado = new TesterRequirementController();


        //<summary> : Shows requirement's index, if idTester is not null, insert into TieneAsginado table a tuple with idTester, projectId and requirementId
        //<param>   : string idTester : If it is not null, it takes it to be able to insert it as part of the new tuple, 
        //            it is only valid when a requirement with an assigned tester is created
        //            int? projectId: the id of the project related with the requirement. 
        //<return>  : Redirect to Index View
        public ActionResult Index( int? projectId, string idTester)
        {
            if (idTester != null && idTester != "")
            {
                tieneAsignado.insert(idTester, projectId);
            }
            var requerimientoes = db.Requerimientoes.Include( r => r.Proyecto ).Where( r => r.id_proyectoFK == projectId ).OrderByDescending(r => r.idPK);
            ViewBag.projectId = projectId ;
            ViewBag.projectName = projectController.getProjectName( projectId );
            return View( requerimientoes.ToList( ) );
        }

        //<summary> : Shows requirement's details
        //<param>   : int? id : this parameter is an identifier for the requirement that will be displayed from the database
        //            int? projectId: the id of the project related with the requirement. 
        //<return>  : Redirect to Details View
        public async Task<ActionResult> Details( int? id , int? projectId )
        {
            if( id == null || projectId == null )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            Requerimiento requerimiento = await db.Requerimientoes.FindAsync( id , projectId );
            if ( requerimiento == null )
            {
                return HttpNotFound( );
            }
            ViewBag.fechaAsignacion = dateTimeToString( requerimiento.fechaAsignacion , "MM/dd/yyyy" );
            ViewBag.fechaFin = dateTimeToString( requerimiento.fechaFinalizacion , "MM/dd/yyyy" );
            ViewBag.fechaInicio = dateTimeToString( requerimiento.fechaInicio , "MM/dd/yyyy" );
            SelectListItem tester = getTester(projectId, id);
            ViewBag.tester = "";
            if (tester != null)
            {
                ViewBag.tester = tester.Text;
            }
            return View( requerimiento );
        }

        //<summary> : GET --> Shows the input form to create a new requirement 
        //<param>   : int? projectId: is the id of the project related with the new requirement. 
        //<return>  : Redirect to Create view
        public ActionResult Create(int? projectId )
        {
            ViewBag.id_proyectoFK = new SelectList( db.Proyectoes , "idPK" , "nombre" );
            ViewBag.projectId = projectId;
            ViewBag.testers = getTesters(projectId);
            return View( );
        }

        //<summary> : POST --> Adds the new requirement to the db
        //<param>   : Requerimiento requerimiento: is the model from input form with the necessary information to create a new requirement
        //            FormCollection fc: Set of view forms to be able to extract stored data
        //<return>  : Redirect to requirement Index if the requirement waa successfully added or shows an error otherwhise
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create( [Bind( Include = "idPK,nombre,id_proyectoFK,fechaInicio,fechaFinalizacion,fechaAsignacion,estado,complejidad,descripcion,duracionEstimada,duracionReal" )] Requerimiento requerimiento , FormCollection fc)
        {
            int? projectId = requerimiento.id_proyectoFK;
            if( ModelState.IsValid )
            {
                string idTester = fc["idTester"];
                db.Requerimientoes.Add( requerimiento );
                db.SaveChanges( );
                return RedirectToAction( "Index" , new {projectId, idTester } );
            }

            ViewBag.id_proyectoFK = new SelectList( db.Proyectoes , "idPK" , "nombre" , requerimiento.id_proyectoFK );
            return View( requerimiento );
        }

        //<summary> : Shows requirement's edit,
        //<param>   : int? id : this parameter is an identifier for the requirement that will be displayed from the database
        //            int? projectId: the id of the project related with the requirement. 
        //<return>  : Redirect to Details View
        public async Task<ActionResult> Edit( int? id , int? projectId )
        {
            if( id == null || projectId == null )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            Requerimiento requerimiento = await db.Requerimientoes.FindAsync( id , projectId );
            if( requerimiento == null )
            {
                return HttpNotFound();
            }
            SelectListItem actualTester = getTester(projectId, id);
            List<SelectListItem> allTesters = getTesters(projectId);
            ViewBag.defaultText = "Seleccione un tester";
            if(actualTester != null)
            {
                foreach (SelectListItem tester in allTesters)
                {
                    if (tester.Value == actualTester.Value)
                    {
                        ViewBag.defaultText = tester.Text;
                        break;
                    }
                }
            }
            
            ViewBag.testers = allTesters;
            ViewBag.fechaAsignacion = dateTimeToString( requerimiento.fechaAsignacion , "MM/dd/yyyy" );
            ViewBag.fechaFin = dateTimeToString( requerimiento.fechaFinalizacion , "MM/dd/yyyy" );
            ViewBag.fechaInicio = dateTimeToString( requerimiento.fechaInicio , "MM/dd/yyyy" );
            ViewBag.id_proyectoFK = new SelectList( db.Proyectoes , "idPK" , "nombre" , requerimiento.id_proyectoFK );
            return View( requerimiento );
        }

        //<summary> : POST --> Modify the requirement to the db
        //<param>   : Requerimiento requerimiento: is the model from input form with the necessary information to edit a requirement
        //            FormCollection fc: Set of view forms to be able to extract stored data
        //<return>  : Redirect to requirement Index if the requirement waa successfully edited or redirect to edit again otherwhise
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( [Bind( Include = "idPK,nombre,id_proyectoFK,fechaInicio,fechaFinalizacion,fechaAsignacion,estado,complejidad,descripcion,duracionEstimada,duracionReal" )] Requerimiento requerimiento, FormCollection fc)
        {

            if( ModelState.IsValid )
            {
                db.Entry( requerimiento ).State = EntityState.Modified;
                db.SaveChanges( );
                string idTester = fc["idTester"];
                if (idTester != "") {
                    db.SP_Actualizar_TieneAsignado(requerimiento.id_proyectoFK, requerimiento.idPK, idTester);
                }
                return RedirectToAction( "Index" , new {projectId = requerimiento.id_proyectoFK} );

            }
            SelectListItem actualTester = getTester(requerimiento.id_proyectoFK, requerimiento.idPK);
            List<SelectListItem> allTesters = getTesters(requerimiento.id_proyectoFK);
            ViewBag.defaultText = "Seleccione un tester";
            if (actualTester != null)
            {
                foreach (SelectListItem tester in allTesters)
                {
                    if (tester.Value == actualTester.Value)
                    {
                        ViewBag.defaultText = tester.Text;
                        break;
                    }
                }
            }
            ViewBag.testers = allTesters;
            ViewBag.fechaAsignacion = dateTimeToString(requerimiento.fechaAsignacion, "MM/dd/yyyy");
            ViewBag.fechaFin = dateTimeToString(requerimiento.fechaFinalizacion, "MM/dd/yyyy");
            ViewBag.fechaInicio = dateTimeToString(requerimiento.fechaInicio, "MM/dd/yyyy");
            ViewBag.id_proyectoFK = new SelectList( db.Proyectoes , "idPK" , "nombre" , requerimiento.id_proyectoFK );
            return View( requerimiento );
        }

        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                db.Dispose( );
            }
            base.Dispose( disposing );
        }


        //------------------------------------METHODS AND FUNCTIONS CREATED BY THE TEAM-------------------------------------

        //<summary> :   It is used to remove a requirement from the database.
        //<param>   :   int id : this parameter is an identifier for the requirement that will be removed from the database
        //              int? projectId : is the id of the project related with the requirement. 
        //<return>  : Redirect to Index,where the requirements appears
        public ActionResult removeRequirement( int id , int? projectId )
        {
            Requerimiento requerimiento = db.Requerimientoes.Find( id , projectId );
            db.Requerimientoes.Remove( requerimiento );
            db.SaveChanges( );
            return RedirectToAction( "Index" , new { projectId = projectId} );
        }

        //<summary> : validates if a name of requirement exists in the database
        //<param>   : string name : the name of the requirement to validates
        //            int idProject :  is the id of the project related with the requirement
        //<return>  : string, a string that represents the correct date
        public bool validateName( string name, int idProject )
        {
            var exist = db.Requerimientoes.Any( req => req.nombre == name &&  req.id_proyectoFK == idProject );
            return exist;
        }

        //<summary> :   Returns the name of the requirement that is associated to the id given.
        //<param>   :   int? id: ID associated to the requirement.
        //<return>  :   The name of the requirement
        public string getRequirementName(int? id)
        {
            List<string> requirement;
            string requirementName = "";
            if (id != null)
            {
                string name = "SELECT R.nombre " + "FROM ControlCalidad.Requerimiento R " + "WHERE R.idPk = " + id;
                requirement = db.Database.SqlQuery<string>(name).ToList();
                requirementName = requirement[0];
            }

            return requirementName;
        }

        //<summary> :   Returns the list of available testers that are associated with the project
        //<param>   :   int? projectId: ID associated with the requirement project
        //<return>  :   List of available testers
        public List<SelectListItem> getTesters(int? projectId) {
            List<SP_Conseguir_testers_req_Result> testers = db.SP_Conseguir_testers_req(projectId).ToList();
            List<SelectListItem> allTesters = testers.ConvertAll(
                tester => {
                    return new SelectListItem()
                    {
                        Text = tester.nombreP,
                        Value = tester.cedulaPK,
                        Selected = false
                    };
                });
            return allTesters;
        }

        //<summary> :   Returns the result of the SP, only one tester returns since only one assigned tester can be
        //<param>   :   int? projectId: ID associated with the requirement project
        //              int? requirementId: ID associated with the requirement
        //<return>  :   Tester assigned to the requirement
        public SelectListItem getTester(int? projectId, int? requirementId)
        {
            SelectListItem tester_selected = null;
            try
            {
                SP_Conseguir_Tester_Result tester = db.SP_Conseguir_Tester(projectId, requirementId).Single();
                tester_selected = new SelectListItem()
                {
                    Text = tester.nombreP,
                    Value = tester.cedulaPK,
                    Selected = false
                };
            }
            catch (Exception e) { 
                //this happens when there is no tester 
            }
            return tester_selected;
        }

        //<summary> : does a cast for nullable date to datetime
        //<param>   : DateTime? dt : the date to cast
        //            string format : the date's format
        //<return>  : string, a string that represents the correct date
        public string dateTimeToString(DateTime? dt, string format)
        {
            return dt == null ? "" : ( ( DateTime ) dt ).ToString( format );
        }

 
        // <summary>   :   Used to get the requirements that has been associated to a tester in a project. 
        // <param>     :   projectId : the identifier of the project
        // <returns>   :   Returns the requirements of the project that have testers associated.
        public JsonResult getJsonRequirements(int projectId)
        {
            db.Configuration.ProxyCreationEnabled = false;

            List<USP_obtenerReqsAsignados1_Result> allReqs = db.USP_obtenerReqsAsignados1(projectId).ToList();

            return Json(allReqs, JsonRequestBehavior.AllowGet);
        }
    }

}
