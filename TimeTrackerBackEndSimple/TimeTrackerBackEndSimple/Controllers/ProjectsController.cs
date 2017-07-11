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
    public class ProjectsController : ApiController
    {
        private TimeTrackerDBEntities db = new TimeTrackerDBEntities();

        // GET: api/Projects
        public IQueryable<Project> GetProjects()
        {
            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (UserSecurity.IsAdmin(authorizedUser))
                return db.Projects;
            else
                return db.Projects.Where(project =>
                    project.AssignedUser.Equals(authorizedUser.Id));
        }

        // GET: api/Projects/5
        [ResponseType(typeof(Project))]
        public async Task<IHttpActionResult> GetProject(int id)
        {
            Project project = await db.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (UserSecurity.IsAdmin(authorizedUser) ||
                project.AssignedUser.Equals(authorizedUser.Id))
            {
                return Ok(project);
            }
            else
                return StatusCode(HttpStatusCode.NotAcceptable);
        }

        // PUT: api/Projects/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutProject(int id, Project project)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != project.Id)
            {
                return BadRequest();
            }

            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (!UserSecurity.IsAdmin(authorizedUser))
                return StatusCode(HttpStatusCode.NotAcceptable);

            db.Entry(project).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
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

        // POST: api/Projects
        [ResponseType(typeof(Project))]
        public async Task<IHttpActionResult> PostProject(Project project)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (!UserSecurity.IsAdmin(authorizedUser))
                return StatusCode(HttpStatusCode.NotAcceptable);

            if(project.Owner != authorizedUser.Id)
            {
                return BadRequest();
            }

            db.Projects.Add(project);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProjectExists(project.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = project.Id }, project);
        }

        // DELETE: api/Projects/5
        [ResponseType(typeof(Project))]
        public async Task<IHttpActionResult> DeleteProject(int id)
        {

            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (!UserSecurity.IsAdmin(authorizedUser))
                return StatusCode(HttpStatusCode.NotAcceptable);

            Project project = await db.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            db.Projects.Remove(project);
            await db.SaveChangesAsync();

            return Ok(project);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProjectExists(int id)
        {
            return db.Projects.Count(e => e.Id == id) > 0;
        }
    }
}