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
    public class ClientController : Controller
    {
        private localizationsController localizations = new localizationsController( );

        private QASystemEntities db = new QASystemEntities( );
        private static string editID;
        private static string mail;

        //<summary> : gets clients from database to put them in a list
        //<param>   : None
        //<return>  : List<SelectListItem>, a client list
        public List<SelectListItem> GetClients()
        {
            List<ClientForproject> clientsList =
                ( from client in db.Clientes
                  select new ClientForproject {
                      cedulaPK = client.cedulaPK ,
                      nombreP = client.nombreP ,
                      apellido1 = client.apellido1 ,
                      apellido2 = client.apellido2 ,
                      nombreCompleto = client.nombreP + " " + client.apellido1 + " " + client.apellido2


                  } ).ToList( );

            List<SelectListItem> allClients = clientsList.ConvertAll(
                client => {
                    return new SelectListItem( ) {
                        Text = client.nombreCompleto ,
                        Value = client.cedulaPK.ToString( ) ,
                        Selected = false
                    };
                } );

            return allClients;
        }

        // GET: Client
        public async Task<ActionResult> Index()
        {

            return View( await db.Clientes.ToListAsync( ) );
        }

        // GET: Client/Details/5
        public async Task<ActionResult> Details( string id )
        {
            if( id == null )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            Cliente cliente = await db.Clientes.FindAsync( id );
            if( cliente == null )
            {
                return HttpNotFound( );
            }
            return View( cliente );
        }

        // GET: Client/Create
        public ActionResult Create()
        {
            ViewBag.provinces = this.localizations.provinceList( );
            return View( );

        }

        // POST: Client/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> Create( [Bind( Include = "cedulaPK,nombreP,apellido1,apellido2,telefono,correo,provincia,canton,distrito,direccionExacta,fechaNacimiento" )] Cliente cliente )
        {
            string provinceName = localizations.provinceName( cliente.provincia );
            string cantonName = localizations.cantonName( cliente.provincia , cliente.canton );
            string districtName = localizations.districtName( cliente.provincia , cliente.canton , cliente.distrito );
            cliente.provincia = provinceName;
            cliente.canton = cantonName;
            cliente.distrito = districtName;
            if( ModelState.IsValid )
            {
                db.Clientes.Add( cliente );
                try
                {
                    await db.SaveChangesAsync( );
                }

                catch
                {
                    ModelState.AddModelError( "" , "No pudo crear cliente" );
                    return View( cliente );

                }
                return RedirectToAction( "Index" );

            }

            return View( cliente );
        }

        // GET: Client/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            ViewBag.provinces = this.localizations.provinceList();
            editID = string.Copy(id);
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cliente cliente = await db.Clientes.FindAsync(id);
            mail = cliente.correo;
            if (cliente == null)
            {
                return HttpNotFound();
            }
            return View(cliente);

        }

        // POST: Client/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<ActionResult> Edit( [Bind( Include = "cedulaPK,nombreP,apellido1,apellido2,telefono,correo,provincia,canton,distrito,direccionExacta,fechaNacimiento" )] Cliente cliente )
        {

            var editProvince = "";
            var editCanton = "";
            var editDistrict = "";

            if (cliente.provincia != null)
            {
                editProvince = Regex.Match(cliente.provincia, @"\d+").Value;
            }
            if (cliente.provincia != null)
            {
                editCanton = Regex.Match(cliente.canton, @"\d+").Value;
            }
            if (cliente.provincia != null)
            {
                editDistrict = Regex.Match(cliente.distrito, @"\d+").Value;
            }
            

            string provinceName = null;
            string cantonName = null;
            string districtName = null;
            string provinceID = null;

            if (editProvince != "")
            {
                provinceName = localizations.provinceName(cliente.provincia);
                cliente.provincia = provinceName;

            }
            if (editCanton != "")
            {
                provinceID = this.localizations.provinceID(provinceName).ToString();
                cantonName = localizations.cantonName(provinceID, cliente.canton);
                cliente.canton = cantonName;

            }
            if (editDistrict != "")
            {
                string cantonID = this.localizations.cantonID(provinceName, cliente.canton).ToString();
                districtName = localizations.districtName(provinceID, cantonID, cliente.distrito);
                cliente.distrito = districtName;
            }

            if ( ModelState.IsValid )
            {
                db.Edit_Cliente(editID, cliente.cedulaPK, cliente.nombreP, cliente.apellido1, cliente.apellido2,
                                 cliente.telefono, cliente.correo, cliente.provincia,
                                 cliente.canton, cliente.distrito, cliente.direccionExacta, cliente.fechaNacimiento);


                return RedirectToAction("Edit", new {id=cliente.cedulaPK});
            }
            return View( cliente );
        }


        // GET: Client/Delete/5
        public async Task<ActionResult> Delete( string id )
        {
            if( id == null )
            {
                return new HttpStatusCodeResult( HttpStatusCode.BadRequest );
            }
            Cliente cliente = await db.Clientes.FindAsync( id );
            if( cliente == null )
            {
                return HttpNotFound( );
            }
            return View( cliente );
        }

        // POST: Client/Delete/5
        [HttpPost, ActionName( "Delete" )]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed( string id )
        {
            Cliente cliente = await db.Clientes.FindAsync( id );
            db.Clientes.Remove( cliente );
            await db.SaveChangesAsync( );
            return RedirectToAction( "Index" );
        }

        //<summary> :   It is used to remove a client from the database.
        //<param>   :   clientID, this parameter identifies the client that will be removed from the database. 
        //<return>  :   Redirect to Index.
        public ActionResult RemoveClient( string clientId )
        {
            Cliente client = db.Clientes.Find( clientId );
            db.Clientes.Remove( client );
            db.SaveChanges( );
            return RedirectToAction( "Index" );
        }

        //<summary> :   Returns de id of a client associated to the email given.
        //<param>   :   string email:   The email of the client.
        //<return>  :   The id of a client.
        public string GetClientIdByEmail(string email) {
            string id;

            List<Cliente> client = db.Clientes.Where(c => c.correo.Equals(email)).ToList();
            if (client[0] != null)
            {
                id = client[0].cedulaPK;
            }
            else {
                id = "";
            }
            

            return id;
        }

        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                db.Dispose( );
            }
            base.Dispose( disposing );
        }

        //<summary> : This method is used to know if one email has been taken from another client or employee.
        //<params>  : input : It's the email we want to validate if is taken or not.
        //<return>  : Returns a boolean value, true if the email was taken, false the otherwise.
        public bool isMailTaken(string input)
        {
            if (mail == input) {
                return false;
            }else{
                var exist = db.Clientes.Any(x => x.correo == input);
                return exist;

            }
        }
        //<summary> : This method is used to verify if an ID has already been taken from another client.
        //<params>  : input : It's the ID we want to validate if is taken or not.
        //<return>  : Returns a boolean value, true if the ID was registered, false the otherwise.
        public bool existID(string id)
        {
            List<Cliente> cliente = db.Clientes.Where(x => x.cedulaPK == id).ToList();
            if (cliente.Count >= 1)
            {
                return true;
            }

            return false;
        }
    }
}
