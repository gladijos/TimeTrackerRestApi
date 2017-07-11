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
    public class TasksController : ApiController
    {
        private TimeTrackerDBEntities db = new TimeTrackerDBEntities();

        // GET: api/Tasks
        public IQueryable<Models.Task> GetTasks()
        {
            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (UserSecurity.IsAdmin(authorizedUser))
                return db.Tasks;
            else
                return db.Tasks.Where(task => 
                    task.Project.AssignedUser.Equals(authorizedUser.Id));
        }

        // GET: api/Tasks/5
        [ResponseType(typeof(Models.Task))]
        public async Task<IHttpActionResult> GetTask(int id)
        {
            Models.Task task = await db.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (UserSecurity.IsAdmin(authorizedUser) || 
                task.Project.AssignedUser.Equals(authorizedUser.Id))
            {
                return Ok(task);
            }
            else
                return StatusCode(HttpStatusCode.NotAcceptable);

        }

        // PUT: api/Tasks/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutTask(int id, Models.Task task)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != task.Id)
            {
                return BadRequest();
            }

            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (UserSecurity.IsAdmin(authorizedUser))
            {
                db.Entry(task).State = EntityState.Modified;

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskExists(id))
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
            else
                return StatusCode(HttpStatusCode.NotAcceptable);


        }

        // POST: api/Tasks
        [ResponseType(typeof(Models.Task))]
        public async Task<IHttpActionResult> PostTask(Models.Task task)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (UserSecurity.IsAdmin(authorizedUser))
            {
                db.Tasks.Add(task);

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    if (TaskExists(task.Id))
                    {
                        return Conflict();
                    }
                    else
                    {
                        throw;
                    }
                }

                return CreatedAtRoute("DefaultApi", new { id = task.Id }, task);

            }
            else
                return StatusCode(HttpStatusCode.NotAcceptable);
        }

        // DELETE: api/Tasks/5
        [ResponseType(typeof(Models.Task))]
        public async Task<IHttpActionResult> DeleteTask(int id)
        {

            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (UserSecurity.IsAdmin(authorizedUser))
            {
                Models.Task task = await db.Tasks.FindAsync(id);
                if (task == null)
                {
                    return NotFound();
                }

                db.Tasks.Remove(task);
                await db.SaveChangesAsync();

                return Ok(task);

            }
            else
                return StatusCode(HttpStatusCode.NotAcceptable);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TaskExists(int id)
        {
            return db.Tasks.Count(e => e.Id == id) > 0;
        }
    }
}