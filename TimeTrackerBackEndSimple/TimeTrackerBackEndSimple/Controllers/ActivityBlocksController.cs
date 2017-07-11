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
using TimeTrackerBackEndSimple;
using TimeTrackerBackEndSimple.Models;

namespace TimeTrackerBackEndSimple.Controllers
{
    public class ActivityBlocksController : ApiController
    {
        private TimeTrackerDBEntities db = new TimeTrackerDBEntities();

        // GET: api/ActivityBlocks
        public IQueryable<ActivityBlocks> GetActivityBlocks()
        {
            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (UserSecurity.IsAdmin(authorizedUser))
                return db.ActivityBlocks;
            else
                return db.ActivityBlocks.Where(activityBlock =>
                    activityBlock.Task.Project.AssignedUser.Equals(authorizedUser.Id));

        }

        // GET: api/ActivityBlocks/5
        [ResponseType(typeof(ActivityBlocks))]
        public async Task<IHttpActionResult> GetActivityBlocks(int id)
        {
            ActivityBlocks activityBlocks = await db.ActivityBlocks.FindAsync(id);
            if (activityBlocks == null)
            {
                return NotFound();
            }

            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (UserSecurity.IsAdmin(authorizedUser) ||
                activityBlocks.Task.Project.AssignedUser.Equals(authorizedUser.Id))
                return Ok(activityBlocks);
            else
                return StatusCode(HttpStatusCode.NotAcceptable);
        }

        // PUT: api/ActivityBlocks/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutActivityBlocks(int id, ActivityBlocks activityBlocks)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != activityBlocks.Id)
            {
                return BadRequest();
            }

            if ((activityBlocks.Rate <= 0) || (activityBlocks.Rate > 1))
            {
                return BadRequest();
            }

            DateTime currentTime = DateTime.Now;

            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();

            if (activityBlocks.Task.Project.AssignedUser.Equals(authorizedUser.Id))
            {
                if (((currentTime - activityBlocks.StartTime).Hours < 25) &&
                    ((activityBlocks.EndTime - activityBlocks.StartTime).Minutes < 31))
                {
                    return StatusCode(HttpStatusCode.BadRequest);
                }
            }
            if (!activityBlocks.Task.Project.AssignedUser.Equals(authorizedUser.Id) &&
                !UserSecurity.IsAdmin(authorizedUser))
            {
                return StatusCode(HttpStatusCode.BadRequest);
            }
            db.Entry(activityBlocks).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ActivityBlocksExists(id))
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

        // POST: api/ActivityBlocks
        [ResponseType(typeof(ActivityBlocks))]
        public async Task<IHttpActionResult> PostActivityBlocks(ActivityBlocks activityBlocks)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if ((activityBlocks.Rate <= 0) || (activityBlocks.Rate > 1))
            {
                return BadRequest();
            }

            DateTime currentTime = DateTime.Now;

            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();

            if (activityBlocks.Task.Project.AssignedUser.Equals(authorizedUser.Id))
            {
                if (((currentTime - activityBlocks.StartTime).Hours < 25) &&
                    ((activityBlocks.EndTime - activityBlocks.StartTime).Minutes < 31))
                {
                    return StatusCode(HttpStatusCode.BadRequest);
                }
            }
            if (!activityBlocks.Task.Project.AssignedUser.Equals(authorizedUser.Id) &&
                !UserSecurity.IsAdmin(authorizedUser))
            {
                return StatusCode(HttpStatusCode.BadRequest);
            }

            db.ActivityBlocks.Add(activityBlocks);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ActivityBlocksExists(activityBlocks.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = activityBlocks.Id }, activityBlocks);
        }

        // DELETE: api/ActivityBlocks/5
        [ResponseType(typeof(ActivityBlocks))]
        public async Task<IHttpActionResult> DeleteActivityBlocks(int id)
        {
            ActivityBlocks activityBlocks = await db.ActivityBlocks.FindAsync(id);
            if (activityBlocks == null)
            {
                return NotFound();
            }
            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (!UserSecurity.IsAdmin(authorizedUser))
                return StatusCode(HttpStatusCode.NotAcceptable);
            db.ActivityBlocks.Remove(activityBlocks);
            await db.SaveChangesAsync();

            return Ok(activityBlocks);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ActivityBlocksExists(int id)
        {
            return db.ActivityBlocks.Count(e => e.Id == id) > 0;
        }
    }
}