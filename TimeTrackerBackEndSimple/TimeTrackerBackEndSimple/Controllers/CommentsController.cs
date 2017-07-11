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
    public class CommentsController : ApiController
    {
        private TimeTrackerDBEntities db = new TimeTrackerDBEntities();

        // GET: api/Comments
        public IQueryable<Comment> GetComments()
        {
            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (UserSecurity.IsAdmin(authorizedUser))
                return db.Comments;
            else
                return db.Comments.Where(comment =>
                    comment.Task.Project.AssignedUser.Equals(authorizedUser.Id));
        }

        // GET: api/Comments/5
        [ResponseType(typeof(Comment))]
        public async Task<IHttpActionResult> GetComment(int id)
        {
            Comment comment = await db.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (UserSecurity.IsAdmin(authorizedUser) ||
                comment.Task.Project.AssignedUser.Equals(authorizedUser.Id))
                return Ok(comment);
            else
                return StatusCode(HttpStatusCode.NotAcceptable);
        }

        // PUT: api/Comments/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutComment(int id, Comment comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != comment.Id)
            {
                return BadRequest();
            }
            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (UserSecurity.IsAdmin(authorizedUser) ||
                comment.Task.Project.AssignedUser.Equals(authorizedUser.Id))
            {
                comment.DateTimeLastModify = DateTime.Now;
                db.Entry(comment).State = EntityState.Modified;

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(id))
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

        // POST: api/Comments
        [ResponseType(typeof(Comment))]
        public async Task<IHttpActionResult> PostComment(Comment comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (UserSecurity.IsAdmin(authorizedUser) ||
                comment.Task.Project.AssignedUser.Equals(authorizedUser.Id))
            {
                comment.DateTimeLastModify = DateTime.Now;
                db.Comments.Add(comment);

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    if (CommentExists(comment.Id))
                    {
                        return Conflict();
                    }
                    else
                    {
                        throw;
                    }
                }

                return CreatedAtRoute("DefaultApi", new { id = comment.Id }, comment);

            }
            else
                return StatusCode(HttpStatusCode.NotAcceptable);
        }

        // DELETE: api/Comments/5
        [ResponseType(typeof(Comment))]
        public async Task<IHttpActionResult> DeleteComment(int id)
        {
            Comment comment = await db.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            User authorizedUser = UserSecurity.GetCurrentAuthorizedUser();
            if (UserSecurity.IsAdmin(authorizedUser) ||
                comment.Task.Project.AssignedUser.Equals(authorizedUser.Id))
            {
                db.Comments.Remove(comment);
                await db.SaveChangesAsync();

                return Ok(comment);
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

        private bool CommentExists(int id)
        {
            return db.Comments.Count(e => e.Id == id) > 0;
        }
    }
}