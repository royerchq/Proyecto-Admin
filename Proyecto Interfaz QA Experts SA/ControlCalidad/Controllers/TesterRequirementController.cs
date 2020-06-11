using ControlCalidad.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ControlCalidad.Controllers
{
    public class TesterRequirementController : Controller
    {

        private QASystemEntities db = new QASystemEntities( );

        //Controller to get information from test
        private TestController testController = new TestController( );

        //<summary> : verifies if a requirement is assigned to any tester (Uses a Stored Procedure from teh db)
        //<param>   : int id :  requirement's identifier . 
        //<return>  : return 1 if the requirement is assigned to any tester, otherwise return 0
        public int? isAssignedToTester(int id)
        {
            int? assigned = 0;
            List<int?> isAssigned = db.USP_estaAsignadoR( id ).ToList();
            assigned = isAssigned[ 0 ];
            return assigned;
        }

        //<summary> : validates if a requirement can be deleted from the db (if the requirement does not has related tester or tests) 
        //<param>   : int id :  requirement's identifier . 
        //<return>  : true if it can be deleted, otherwise return false 
        public bool canDelete(int id)
        {
            bool deleted = false;

            if( testController.isAssigned( id ) == false && isAssignedToTester( id ) == 0 )
            {
                deleted = true;
            }

            return deleted;

        }

        //<summary> :   Insert a new tuple inside the TieneAsignado table
        //<param>   :   string cedula_empeladoFK: ID of the tester that you want to associate with the requirement
        //              int? id_proyectoFK: ID of the project that you want to associate with the requirement
        public void insert(string cedula_empeladoFK, int? id_proyectoFK)
        {
            TieneAsignado newEntity = new TieneAsignado
            {
                cedula_empleadoFK = cedula_empeladoFK,
                id_requerimientoFK = db.Database.SqlQuery<int>("SELECT [ControlCalidad].[UFN_getId]()").Single(),
                id_proyectoFK = Convert.ToInt32(id_proyectoFK),
                horasDedicas = 0
            };
            db.TieneAsignadoes.Add(newEntity);
            db.SaveChanges();
        }

        //<summary> :   Insert a new tuple inside the TieneAsignado table
        //<param>   :   string cedula_empeladoFK: ID of the tester that belongs to the tuple that you want to remove
        //              int? id_proyectoFK: Id of the project that belongs to the tuple that you want to remove
        //              int? id_requerimiento: Id of the requirement that belongs to the tuple that you want to remove
        public void delete(string cedula_empeladoFK, int? id_proyectoFK, int? id_requerimiento)
        {
            TieneAsignado newEntity = db.TieneAsignadoes.Find(cedula_empeladoFK, Convert.ToInt32(id_proyectoFK), Convert.ToInt32(id_requerimiento));

        }
    }
}