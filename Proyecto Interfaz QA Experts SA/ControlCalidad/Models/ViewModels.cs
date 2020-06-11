using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ControlCalidad.Models
{
    public class ViewModels
    {
    }

    public class ClientForproject
    {
        public string cedulaPK
        {
            get; set;
        }
        public string nombreP
        {
            get; set;
        }
        public string apellido1
        {
            get; set;
        }
        public string apellido2
        {
            get; set;
        }

        public string nombreCompleto
        {
            get; set;
        }

    }

    public class ProjectForReports
    {
        public string nombre
        {
            get; set;
        }

        public int idPk
        {
            get; set;
        }

    }


    public class ProjectsForReports
    {
        public string nombre
        {
            get; set;
        }

    }

    public class FinishedProjectForReports
    {
        public string nombre
        {
            get; set;
        }

        public int idPk
        {
            get; set;

        }
    }


    public class EmployeeForReports
    {
        public string nombreP
        {
            get; set;
        }

    }

    public class EmployeeForReportsT
    {
        public string nombreP
        {
            get; set;
        }

    }

    public class Localizations
    {
        public string nombreProvincia
        {
            get; set;
        }
        public int numProvincia
        {
            get; set;
        }
        public string nombreCanton
        {
            get; set;
        }
        public int numCanton
        {
            get; set;
        }
        public string nombreDistrito
        {
            get; set;
        }
        public string numDistrito
        {
            get; set;
        }
    }

    public class EmployeesForHabilities
    {
        public string employeeName
        {
            get; set;
        }
        public string employeeID
        {
            get; set;
        }
    }

    public class LeaderForProject {

        public string cedulaPK
        {
            get; set;
        }

        public string nombreP
        {
            get; set;
        }

        public string apellido1
        {
            get; set;
        }

        public string apellido2
        {
            get; set;
        }

        public string nombreCompleto
        {
            get; set;
        }

        public string disponibilidad
        {
            get; set;
        }
    }

    public class Leader
    {
        public string nombreP
        {
            get; set;
        }

        public string apellido1
        {
            get; set;
        }

        public string apellido2
        {
            get; set; 
        }
            
        public string nombreCompleto
        {
            get; set;
        }
    }

    public class CedulaLider
    {
        public string cedulaPK
        {
            get; set;
        }
    }

    public class ProjectId
    {
        public int id_proyectoFK
        {
            get; set;
        }
    }

    public class RequirementForReport
    {
        public int idPk
        {
            get; set;
        }

        public string nombre
        {
            get; set;
        }
    }
}   
