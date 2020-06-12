
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace ControlCalidad.Models
{

using System;
    using System.Collections.Generic;
    
public partial class Requerimiento
{

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
    public Requerimiento()
    {

        this.TieneAsignadoes = new HashSet<TieneAsignado>();

        this.Pruebas = new HashSet<Prueba>();

    }


    public int idPK { get; set; }

    public string nombre { get; set; }

    public int id_proyectoFK { get; set; }

    public System.DateTime fechaInicio { get; set; }

    public Nullable<System.DateTime> fechaFinalizacion { get; set; }

    public Nullable<System.DateTime> fechaAsignacion { get; set; }

    public string estado { get; set; }

    public string complejidad { get; set; }

    public string descripcion { get; set; }

    public float duracionEstimada { get; set; }

    public Nullable<float> duracionReal { get; set; }



    public virtual Proyecto Proyecto { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<TieneAsignado> TieneAsignadoes { get; set; }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]

    public virtual ICollection<Prueba> Pruebas { get; set; }

}

}