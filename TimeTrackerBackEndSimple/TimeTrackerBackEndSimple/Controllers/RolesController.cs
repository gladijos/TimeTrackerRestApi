using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using TimeTrackerBackEndSimple.Models;

namespace TimeTrackerBackEndSimple.Controllers
{
    public class RolesController : ApiController
    {
        private TimeTrackerDBEntities db = new TimeTrackerDBEntities();

        // GET: api/Roles
        public IQueryable<Role> GetRoles()
        {
            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (UserSecurity.IsAdmin(authorizedUser))
                return db.Roles;
            else
                return null;

        }

        // GET: api/Roles/5
        [ResponseType(typeof(Role))]
        public async Task<IHttpActionResult> GetRole(int id)
        {
            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (!UserSecurity.IsAdmin(authorizedUser))
                return StatusCode(HttpStatusCode.NotAcceptable);

            Role role = await db.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            return Ok(role);
        }

        // PUT: api/Roles/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutRole(int id, Role role)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != role.Id)
            {
                return BadRequest();
            }

            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (!UserSecurity.IsAdmin(authorizedUser))
                return StatusCode(HttpStatusCode.NotAcceptable);

            db.Entry(role).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Roles
        [ResponseType(typeof(Role))]
        public async Task<IHttpActionResult> PostRole(Role role)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (!UserSecurity.IsAdmin(authorizedUser))
                return StatusCode(HttpStatusCode.NotAcceptable);

            db.Roles.Add(role);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (RoleExists(role.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = role.Id }, role);
        }

        // DELETE: api/Roles/5
        [ResponseType(typeof(Role))]
        public async Task<IHttpActionResult> DeleteRole(int id)
        {

            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (!UserSecurity.IsAdmin(authorizedUser))
                return StatusCode(HttpStatusCode.NotAcceptable);

            Role role = await db.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            db.Roles.Remove(role);
            await db.SaveChangesAsync();

            return Ok(role);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool RoleExists(int id)
        {
            return db.Roles.Count(e => e.Id == id) > 0;
        }
    }
}