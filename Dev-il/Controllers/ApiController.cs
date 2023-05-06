//using AspNetCore;
using bangsoo.Data;
using bangsoo.Models; //_context.Users() 가 안되고, Users()하려고
using Microsoft.AspNetCore.Mvc;

namespace bangsoo.Controllers {
    public class ApiController : Controller {
        private readonly bangsooContext _context;
        public ApiController(bangsooContext context) {
            _context = context;
        }

        [HttpGet]
        public JsonResult Index() {
            return Json(_context.Boards);
        }

        public JsonResult BoardsCreate() {
            return Json(_context.Boards);
        }

        public JsonResult BoardsRead() {
            return Json(_context.Boards);
        }

        public JsonResult BoardsUpdate() {
            return Json(_context.Boards);
        }

        public JsonResult BoardsDelete() {
            return Json(_context.Boards);
        }
    }
}
