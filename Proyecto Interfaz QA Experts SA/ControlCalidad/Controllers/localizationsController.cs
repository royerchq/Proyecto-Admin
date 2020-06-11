using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ControlCalidad.Models;

namespace ControlCalidad.Controllers
{
    public class localizationsController : Controller
    {
        private localizacoinesEntities db = new localizacoinesEntities();

        // GET: Province
        //<summary> : This method obtain all the provinces in Costa rica
        //<return>  : Returns the pronvince list with select items
        public List<SelectListItem> provinceList()
        {
            List<Provincia> provinces = db.Provincias.ToList();

            List<SelectListItem> provinceList = provinces.ConvertAll(province => { return new SelectListItem() {
                Text = province.nombre,
                Value = province.codigoPK.ToString(),
                Selected = false
                };
             });
            return provinceList;
        }

        // GET: Province
        //<summary> : This method obtain all the provinces in Costa rica for the edit method
        //<return>  : Returns the  a JSON with provinces and the ID
        public JsonResult provinceEditList()
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Provincia> provinceList = db.Provincias.ToList();
            return Json(provinceList, JsonRequestBehavior.AllowGet);

        }
        // GET: Province
        //<summary> : This method obtain the province id based on their name
        //<params> : Name, province name
        //<return>  : Returns the province id
        public int provinceID(string name)
        {
            if(name == "")
            {
                return 0;
            }
            db.Configuration.ProxyCreationEnabled = false;
            List<Provincia> provinceList = db.Provincias.Where(x =>x.nombre == name).ToList();
            return provinceList[0].codigoPK;
        }

        //<summary> : This method obtain the canton id based on their name
        //<params> : Name, province name | canton, canton name
        //<return>  : Returns the canton  id
        public int cantonID(string name, string canton)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Provincia> provinceList = db.Provincias.Where(x => x.nombre == name).ToList();
            int provinceID = 0;
            try
            {
                provinceID = provinceList[0].codigoPK;
            }
            catch
            {
                return 0;
            }
            List<Canton> cantonList = db.Cantons.Where(x => x.provinciaFK == provinceID).ToList();
            for(int i = 0; i< cantonList.Count; ++i)
            {
                if(cantonList[i].nombre == canton)
                {
                    return cantonList[i].codigoPK;
                }

            }
            return 0;
        }



        //<summary> : This method obtain the list of the cantones inside a province
        //<params> : provincia, id from the respective province
        //<return>  : Returns a JSON with the canton name and their id
        public JsonResult cantonesList(int provincia)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Canton> cantonList = db.Cantons.Where(x => x.provinciaFK == provincia).ToList();
            return Json(cantonList, JsonRequestBehavior.AllowGet);

        }

        //<summary> : This method obtain the list of the districts inside a province and insde a canton
        //<params> : provincia, id from the respective province, canton, id from the respective canton
        //<return>  : Returns a JSON with the districts name and their id
        public JsonResult districtsList(int provincia,int canton)
        {
            db.Configuration.ProxyCreationEnabled = false;
            List<Distrito> cantonList = db.Distritoes.Where(x => x.cantonFK == canton && x.provinciaFK == provincia).ToList();
            return Json(cantonList, JsonRequestBehavior.AllowGet);


            //return new SelectList((from provincias in db.Provincias
            //                       select provincias.nombre).ToList());
        }




        //<summary> : This method obtain name of a province based on his id
        //<params> :strprovince, string id
        //<return>  : Returns a string with the name of the province
        public string provinceName(string strProvince)
        {
            if(strProvince == null)
            {
                string empty = " ";
                return empty;
            }

            int provinceIndex = System.Convert.ToInt32(strProvince);
            List<Provincia> provincia = db.Provincias.Where(x => x.codigoPK == provinceIndex).ToList();
            return provincia[0].nombre;
        }
        //<summary> : This method obtain name of a canton based on his id
        //<params> :strprovince, string id, ctrcanton, canton id
        //<return>  : Returns a string with the name of the canton
        public string cantonName(string strProvince,string strCanton)
        {
            if (strProvince == null || strCanton == null)
            {
                string empty = " ";
                return empty;
            }
            int province = System.Convert.ToInt32(strProvince);
            int cantonIndex = System.Convert.ToInt32(strCanton);
            List<Canton> canton = db.Cantons.Where(x => x.codigoPK == cantonIndex && x.provinciaFK == province).ToList();
            return canton[0].nombre;
        }
        //<summary> : This method obtain name of a district based on his id
        //<params> :strprovince, string id, ctrcanton, canton id,strdistrict, district id
        //<return>  : Returns a string with the name of the district
        public string districtName(string strProvince,  string strCanton, string strDistrict)
        {
            if (strProvince == null || strCanton == null || strDistrict == null)
            {
                string empty = " ";
                return empty;
            }
            int province = System.Convert.ToInt32(strProvince);
            int canton = System.Convert.ToInt32(strCanton);
            int districtIndex = System.Convert.ToInt32(strDistrict);
            List<Distrito> distrito = db.Distritoes.Where(x => x.codigoPK == districtIndex && x.provinciaFK == province && x.cantonFK == canton).ToList();
            return distrito[0].nombre;
        }



    }
}
