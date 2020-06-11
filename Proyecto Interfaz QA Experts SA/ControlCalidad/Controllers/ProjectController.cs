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
    public class ProjectController : Controller
    {
        private QASystemEntities db = new QASystemEntities( );

        //Controllers to get values from other models
        private ClientController clientController = new ClientController( );
        private EmployeeController employeeController = new EmployeeController( );
        private string projectLeader;


        //<summary> : gets projects from database to put them in a list
        //<param>   : None
        //<return>  : List<SelectListItem>, a projects list
        public List<SelectListItem> GetProjects()
        {
            List<ProjectForReports> ProjectsList =
                (from project in db.Proyectoes
                 select new ProjectForReports
                 {
                     nombre = project.nombre,
                     idPk = project.idPK

                 }).ToList();

            List<SelectListItem> allprojects = ProjectsList.ConvertAll(
                project => {
                    return new SelectListItem()
                    {
                        Text = project.nombre,
                        Value = project.idPk.ToString(),
                        Selected = false
                    };
                });

            return allprojects;
        }

        public List<SelectListItem> GetNameProjects()
        {
            List<ProjectsForReports> NameProjectsList =
                (from project in db.Proyectoes
                 select new ProjectsForReports
                 {
                     nombre = project.nombre,

                 }).ToList();

            List<SelectListItem> allnameprojects = NameProjectsList.ConvertAll(
                project => {
                    return new SelectListItem()
                    {
                        Text = project.nombre,
                        Selected = false
                    };
                });

            return allnameprojects;
        }


        //<summary> : gets the finished projects from database to put them in a list
        //<param>   : None
        //<return>  : List<SelectListItem>, finished projects list
        public List<SelectListItem> GetFinishedProjects()
        {
            List<FinishedProjectForReports> ProjectsList =
                ( from project in db.Proyectoes
                  where project.estado == "Finalizado"
                  select new FinishedProjectForReports {
                      nombre = project.nombre ,
                      idPk = project.idPK
                  } ).ToList( );

            List<SelectListItem> allFinishedprojects = ProjectsList.ConvertAll(
                project => {
                    return new SelectListItem( ) {
                        Text = project.nombre ,
                        Value = project.idPk.ToString(),
                        Selected = false
                    };
                } );

            return allFinishedprojects;
        }

        // GET: Project
        public async Task<ActionResult> Index()
        {
            var clientController = new ClientController();
            var proyectoes = db.Proyectoes.Include( p => p.Cliente ).OrderByDescending(p => p.idPK);
            string email = User.Identity.Name;
            if (User.IsInRole("Tester") || User.IsInRole("Lider"))
            {
                ViewBag.projectId = GetProjectIdByEmail(email);
            }
            else {
                if ( User.IsInRole("Cliente") ) {
                    ViewBag.clientId = clientController.GetClientIdByEmail(email);
                } 
            }

            return View( await proyectoes.ToListAsync( ) );

        }

        // GET: Project/Details/5
        public async Task<ActionResult> Details( int? id )
        {
            if( id == null )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            Proyecto proyecto = await db.Proyectoes.FindAsync( id );
            if( proyecto == null )
            {
                return HttpNotFound( );
            }
            ViewBag.projectLeader = GetLeaderName( id );
            return View( proyecto );
        }

        // GET: Project/Create
        public ActionResult Create()
        {
            ViewBag.leaders = employeeController.GetLeaders( );
            ViewBag.allClientsId = clientController.GetClients( );
            // ViewBag.cedulaClienteFK = new SelectList(db.Clientes, "cedulaPK", "nombreP");
            return View( );
        }

        // POST: Project/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create( [Bind( Include = "idPK,nombre,objetivo,fechaInicio,fechaFin,estado,duracionEstimada,duracionReal,cedulaClienteFK" )] Proyecto proyecto , string cedula_empleadoFK )
        {
            //Console.WriteLine(cedula_empleadoFK);

            if( ModelState.IsValid )
            {

                db.Proyectoes.Add( proyecto );
                await db.SaveChangesAsync( );
                SetLeaderToProject( cedula_empleadoFK , proyecto.idPK , "Lider" );
                ViewBag.leader = cedula_empleadoFK;
                return RedirectToAction( "Index" );
            }

            ViewBag.cedulaClienteFK = new SelectList( db.Clientes , "cedulaPK" , "nombreP" , proyecto.cedulaClienteFK );
            return View( proyecto );
        }

        // GET: Project/Edit/5
        public async Task<ActionResult> Edit( int? id )
        {
            if( id == null )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            Proyecto proyecto = await db.Proyectoes.FindAsync( id );
            if( proyecto == null )
            {
                return HttpNotFound( );
            }

            ViewBag.leaders = employeeController.GetLeaders( );
            ViewBag.allClientsId = clientController.GetClients( );
            ViewBag.cedulaClienteFK = proyecto.cedulaClienteFK;
            return View( proyecto );
        }

        // POST: Project/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit( [Bind( Include = "idPK,nombre,objetivo,fechaInicio,fechaFin,estado,duracionEstimada,duracionReal,cedulaClienteFK" )] Proyecto proyecto , string newProjectLeader )
        {

            if( ModelState.IsValid )
            {
                db.Entry( proyecto ).State = EntityState.Modified;
                EditProjectLeader( newProjectLeader , proyecto.idPK );

                await db.SaveChangesAsync( );
                if (proyecto.estado.Equals("Finalizado") || proyecto.estado.Equals("Cancelado"))
                {
                    db.Libera_Empleado(proyecto.idPK);
                }


                return RedirectToAction( "Index" );
            }
            ViewBag.cedulaClienteFK = new SelectList( db.Clientes , "cedulaPK" , "nombreP" , proyecto.cedulaClienteFK );
            return View( proyecto );
        }

        public ActionResult EditProject( [Bind( Include = "idPK,nombre,objetivo,fechaInicio,fechaFin,estado,duracionEstimada,duracionReal,cedulaClienteFK" )] Proyecto proyecto )
        {
            if( ModelState.IsValid )
            {

                db.Entry( proyecto ).State = EntityState.Modified;
                db.SaveChanges( );
                return RedirectToAction( "Index" );
            }
            ViewBag.cedulaClienteFK = new SelectList( db.Clientes , "cedulaPK" , "nombreP" , proyecto.cedulaClienteFK );
            return RedirectToAction( "Index" );
        }


        // GET: Project/Delete/5
        public async Task<ActionResult> Delete( int? id )
        {
            if( id == null )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            Proyecto proyecto = await db.Proyectoes.FindAsync( id );
            if( proyecto == null )
            {
                return HttpNotFound( );
            }
            return View( proyecto );
        }

        // POST: Project/Delete/5
        [HttpPost, ActionName( "Delete" )]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed( int id )
        {
            Proyecto proyecto = await db.Proyectoes.FindAsync( id );
            db.Proyectoes.Remove( proyecto );
            await db.SaveChangesAsync( );
            return RedirectToAction( "Index" );
        }

        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                db.Dispose( );
            }
            base.Dispose( disposing );
        }

        //-------------------------------------- FUNCTIONS AND METHODS CREATED BY THE TEAM -----------------------------------------------

        public string getProjectName(int? id)
        {
            string name = "SELECT P.nombre " +"FROM ControlCalidad.Proyecto P " + "WHERE P.idPk = " + id;
            List<string> projectName = db.Database.SqlQuery<string>( name ).ToList( );
            return projectName[ 0 ];
        }

        //<summary> :   It is used to remove a project from the database.
        //<param>   :   The id,this parameter is an identifier for the project that will be removed from the database. 
        //<return>  : Redirect to Index,where the project appears
        public ActionResult RemoveProject( int id )
        {
            Proyecto project = db.Proyectoes.Find( id );
            db.Proyectoes.Remove( project );
            db.SaveChanges( );
            return RedirectToAction( "Index" );
        }

        //<summary> :   It is used to validates the status of a project.
        //<param>   :   id,this parameter is an identifier for the project. . 
        //<return>  : projectStatus,the status of the project
        public string activeProject(string id )
        {
            string status = "SELECT	P.estado " +
                "FROM ControlCalidad.Proyecto P " +
                "WHERE P.idPk = " + id + ";";
            List<string> projectStatus = db.Database.SqlQuery<string>( status ).ToList( );
            return projectStatus[ 0 ];
        }

        //<summary> : Validates if a mail exist in the database
        //<param>   : the mail for validate in the database
        //<return>  : true if mail exist in the database,false otherwise
        public bool validateName( string name )
        {
            var exist = db.Proyectoes.Any( project => project.nombre == name );
            return exist;
        }

        //<summary> : This method is used to know the name of an especific leader just by passing his identifier.
        //<param>   : id : The identifier of the leader, it could be null if it's not beign updated.
        //<return>  Returns the name of the leader.
        public string GetLeaderName( int? id )
        {
            string projectLeader = "";
            if( id != null )
            {
                string query = "SELECT	E.nombreP, E.apellido1, E.apellido2 FROM ControlCalidad.TrabajaEn TE JOIN ControlCalidad.Empleado E ON E.cedulaPK = TE.cedula_empleadoFK " +
                    "WHERE TE.id_proyectoFK = " + id + " AND TE.rol = 'Lider';";
                List<Leader> leader = db.Database.SqlQuery<Leader>( query ).ToList( );
                foreach( Leader l in leader )
                {
                    l.nombreCompleto = l.nombreP + " " + l.apellido1 + " " + l.apellido2;

                }
                if( leader.Count( ) > 0 )
                {
                    Leader leaderForProject = leader.Last( );
                    projectLeader = leaderForProject.nombreCompleto;
                }
            }
            return projectLeader;
        }

        //<summary> : This method is in charge to set the new leader to an especific project
        //<param>   : cedula_empleadoFK : Represents the identifier of the leader.
        //<param>   : idPK              : Represente the identifier of the project involved.
        //<param>   : rol               : Represents the role of the employee, in this case always be "Leader"
        //<return>  : None
        public void SetLeaderToProject( string cedula_empleadoFK , int idPK , string rol )
        {
            if( cedula_empleadoFK != "" )
            {
                var TrabajaEn = new TrabajaEn {
                    cedula_empleadoFK = cedula_empleadoFK ,
                    id_proyectoFK = idPK ,
                    rol = rol
                };
                var employee = db.Empleadoes.Find( cedula_empleadoFK );
                employee.disponibilidad = employee.disponibilidad.Replace( "Disponible" , "Ocupado" );

                db.TrabajaEns.Add( TrabajaEn );

                db.SaveChanges( );
            }
        }

        //<summary> : this method is used to edit or add a new leader dependig on if is there a leader or not.
        //<param>   : newProjectLeader : The identifier of the new leader we want to set.
        //<param>   : id               : The identifier of the project involved.
        //<return>  : None
        public void EditProjectLeader( string newProjectLeader , int id )
        {

            if( newProjectLeader != "" )
            {
                //If the new leader is not empty in the post.
                string projectLeader = "";
                // ------- SQL INJECTION -------------//
                string query = "SELECT	E.cedulaPK FROM ControlCalidad.TrabajaEn TE JOIN ControlCalidad.Empleado E ON E.cedulaPK = TE.cedula_empleadoFK " +
                    "WHERE TE.id_proyectoFK = " + id + " AND TE.rol = 'Lider';";
                
                //List with de id of the leader.
                List<CedulaLider> leader = db.Database.SqlQuery<CedulaLider>( query ).ToList( );
                if( leader.Count( ) > 0 )
                {
                    CedulaLider leaderForProject = leader.Last( );
                    projectLeader = leaderForProject.cedulaPK;


                    //Updates the status of the previous leader to 'Available'
                    var employee = db.Empleadoes.Find( projectLeader );
                    employee.disponibilidad = employee.disponibilidad.Replace( "Ocupado" , "Disponible" );
                    //Extracts the tuple of the leader in the team.
                    var leaderInfo = db.TrabajaEns.Find( employee.cedulaPK , id );
                    db.TrabajaEns.Remove( leaderInfo );

                    SetLeaderToProject( newProjectLeader , id , "Lider" );
                }
                else
                {
                    SetLeaderToProject( newProjectLeader , id , "Lider" );
                }

                db.SaveChanges( );
            }

        }

        //Hacer el mismo metodo para jalar los clientes.
        //<summary> : This method is used to get a project from where an employee is working by only passing the email of the employee.
        //<params>  : email : The email of the employee that we want to no know in wich project is working.
        //<return>  : Returns the identifier of the project in wich is involved.
        public int GetProjectIdByEmail(string email)
        {
            int projectId = 0;
            if( email != null )
            {
                //We need the identifier of the employee to execute the query.
                string idEmployee = employeeController.GetEmployeeIdByEmail( email );

                string query = "SELECT	TE.id_proyectoFK FROM ControlCalidad.TrabajaEn TE " +
                    "WHERE TE.cedula_empleadoFK = '" + idEmployee + "'";

                List<ProjectId> projectIdList = db.Database.SqlQuery<ProjectId>( query ).ToList( );
                if( projectIdList.Count( ) > 0 )
                {
                    var project = projectIdList.Last( );
                    projectId = project.id_proyectoFK;
                }
            }
            return projectId;
        }
    }
}
