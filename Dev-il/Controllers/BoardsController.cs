using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using bangsoo.Data;
using bangsoo.Models;
using Microsoft.AspNetCore.Authorization;
using static bangsoo.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using NuGet.Protocol;
using System.Xml;

namespace bangsoo.Controllers
{
    // 전체에 Authorize 적용
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class BoardsController : Controller
    {
        private readonly bangsooContext _context;

        public BoardsController(bangsooContext context)
        {
            _context = context;
        }


        // 페이지네이션 적용
        /// <summary>
        /// Board에 있는 모든 글 Json 리스트로 반환.
        /// </summary>
        /// <returns>Board List Json</returns>
        [Route("api/Boards/GetAllBoards")]
        [HttpGet]
        public async Task<IActionResult> GetAllBoards(int boardType = 0, int page = 0) {

            var boardList = 
                await _context.Boards
                .Where(b => b.BoardType == boardType)
                .Skip(page*20)
                .Take(20)
                .ToListAsync();

            var returnBoardList = new List<SimpleBoardsForm>();


            foreach(var b in boardList) {

                var tmpboard = new SimpleBoardsForm {
                    BoardId = b.BoardId,
                    NickName = b.User.NickName,
                    Title = b.Title,
                    ViewCount = b.ViewCount,
                    WriteTime = b.WriteTime,
                    CommentsCount = b.Comments.Count(), //댓글내용 전부 로드후 카운트함. 비효율적일 수 있음.
                    IsDeleted = b.IsDeleted, // 삭제되었는지 확인 가능
                };

                returnBoardList.Add(tmpboard);
            }
            return Ok(returnBoardList);
        }



        // XSS Attack 관련해서는 Front와 상의해봐야 함 (꺾쇠괄호 처리 등)
        // >> Front에서 전부 처리하기로 함.
        /// <summary>
        /// 글쓰기
        /// </summary>
        /// <param name="postBoard">
        /// <list type="bullet">Title : 제목</list>
        /// <list type="bullet">Contents : 내용</list>
        /// </param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Boards/CreateBoard")]
        public async Task<IActionResult> CreateBoard([FromBody] CreateBoardForm postBoard) {

            if (!ModelState.IsValid) {

                return BadRequest(new { errormsg = "InvalidRequest." });
            }

            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
            Users targetUser = CustomHelpers.WhoThatAuthUser(authorizationHeader, _context);

            if (targetUser != null) {

                var newBoard = new Boards {
                    Title = postBoard.Title,
                    Contents = postBoard.Contents,
                    NickName = targetUser.NickName,
                    BoardType = postBoard.BoardType
                };

                // context에 저장후 DB업데이트
                _context.Boards.Add(newBoard);
                await _context.SaveChangesAsync();

                return Ok(new { msg = "Create Complate." });

            } else {
                return Unauthorized(new { errormsg = "Bad Auth." });
            }
        }

        /// <summary>
        /// 글읽기, 해당글의 댓글도 반환
        /// </summary>
        /// <param name="boardId">글 ID</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Boards/ReadBoard")]
        public async Task<IActionResult> ReadBoard(int boardId) {

            var targetBoard = await _context.Boards.FindAsync(boardId);
            if (targetBoard == null) {

                return BadRequest(new { errormsg = "There is no Board matched Id."});

            } else if (targetBoard.IsDeleted) {

                return BadRequest(new { errormsg = "This Board is deleted."});
            }
            //var targetComments = _context.Comments.Where(c => c.BoardId == boardId);
            var targetComments = targetBoard.Comments;
            List<ReadCommentsForm> returnComments = new();

            foreach (var comment in targetComments) {
                string tmpContents;
                if (comment.IsDeleted) {
                    tmpContents = "삭제된 댓글입니다.";
                } else {
                    tmpContents = comment.Contents;
                }
                if (comment.ReplyCid == 0) {

                    ReadCommentsForm tmpComment = new ReadCommentsForm {
                        CommentId = comment.CommentId,
                        BoardId = comment.BoardId,
                        ReplyCid = comment.ReplyCid,
                        Depth = comment.Depth,
                        IsDeleted = comment.IsDeleted,
                        NickName = comment.NickName,
                        Contents = tmpContents,
                        WriteTime = comment.WriteTime,
                        ModifiedTime = comment.ModifiedTime
                    };
                    returnComments.Add(tmpComment);
                } else {
                    ReadCommentsForm parentComment = returnComments.Find(x => x.CommentId == comment.ReplyCid);
                    int idx = returnComments.IndexOf(parentComment);

                    for (int j= idx+1; j < returnComments.Count; j++) {
                        if (returnComments[j].Depth > parentComment.Depth) {
                            idx++;
                        } else {
                            break;
                        }
                    }

                    ReadCommentsForm tmpComment = new ReadCommentsForm {
                        CommentId = comment.CommentId,
                        BoardId = comment.BoardId,
                        ReplyCid = comment.ReplyCid,
                        Depth = comment.Depth,
                        IsDeleted = comment.IsDeleted,
                        NickName = comment.NickName,
                        Contents = tmpContents,
                        WriteTime = comment.WriteTime,
                        ModifiedTime = comment.ModifiedTime
                    };
                    returnComments.Insert(idx+1, tmpComment);
                }
            }

            // 조회시 뷰카운트 증가
            targetBoard.ViewCount += 1;
            await _context.SaveChangesAsync();

            var returnBoard = new BoardDetailForm {
                BoardId = targetBoard.BoardId,
                BoardType = targetBoard.BoardType,
                NickName = targetBoard.NickName,
                Title = targetBoard.Title,
                Contents = targetBoard.Contents,
                ViewCount = targetBoard.ViewCount,
                WriteTime = targetBoard.WriteTime,
                ModifiedTime = targetBoard.ModifiedTime,
                Comments = returnComments
            };

            return Ok(returnBoard);
        }

        /// <summary>
        /// 수정가능한 유저인지 확인 (필요없을 수도 있음)
        /// </summary>
        /// <param name="boardId">글 ID</param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Boards/CanIEditThisBoard")]
        public async Task<IActionResult> CanIEditThisBoard(int boardId) {

            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
            Users targetUser = CustomHelpers.WhoThatAuthUser(authorizationHeader, _context);
            Boards? targetBoard = await _context.Boards.FindAsync(boardId);

            if (targetUser != null && targetBoard != null && targetUser.NickName == targetBoard.NickName) {
                if (!targetBoard.IsDeleted) {  

                    return Ok(new { msg = "You can edit this board." });

                } else {

                    return BadRequest(new { errormsg = "Deleted Board." });
                }
            } else {

                return Unauthorized(new { msg = "Bad Auth." });

            }
        }

        /// <summary>
        /// 글 수정
        /// </summary>
        /// <param name="postBoard">
        /// <list type="bullet">Title : 제목</list>
        /// <list type="bullet">Contents : 내용</list>
        /// </param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/Boards/UpdateBoard")]
        public async Task<IActionResult> UpdateBoard([FromBody] UpdateBoardForm postBoard) {

            if (!ModelState.IsValid) {
                return BadRequest(new { errormsg = "InvalidRequest." });
            }

            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
            Users targetUser = CustomHelpers.WhoThatAuthUser(authorizationHeader, _context);
            Boards? targetBoard = await _context.Boards.FindAsync(postBoard.BoardId);

            if (targetUser != null && 
                targetBoard != null && 
                targetUser.NickName == targetBoard.NickName &&
                !targetBoard.IsDeleted) {

                targetBoard.Title = postBoard.Title;
                targetBoard.Contents = postBoard.Contents;
                targetBoard.ModifiedTime = DateTime.Now;

                await _context.SaveChangesAsync();
                return Ok(new { msg = "Update Complate." });

            } else {

                return BadRequest(new { errormsg = "Bad Request." });
            }
        }

        /// <summary>
        /// 글 삭제
        /// </summary>
        /// <param name="boardId">글 ID</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("api/Boards/DeleteBoard")]
        public async Task<IActionResult> DeleteBoard(int boardId) {

            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
            Users targetUser = CustomHelpers.WhoThatAuthUser(authorizationHeader, _context);
            Boards? targetBoard = await _context.Boards.FindAsync(boardId);

            if (targetUser != null && targetBoard != null && targetUser.NickName == targetBoard.NickName) {

                targetBoard.IsDeleted = true;
                await _context.SaveChangesAsync();

                return Ok(new { msg = "Delete Complate." });

            } else {

                return Unauthorized(new { errormsg = "Bad Auth." });
            }

        }




        /// <summary>
        /// 댓글 쓰기 (답글 포함)
        /// </summary>
        /// <param name="postComment">
        /// <list type="bullet">BoardId : 게시글 지정</list>
        /// <list type="bullet">ReplyCid : 답글여부, 0이면 일반댓글, 댓글의 Cid일 경우 답글</list>
        /// <list type="bullet">Contents : 내용</list>
        /// </param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Boards/CreateComment")]
        public async Task<IActionResult> CreateComment([FromBody] CreateComments postComment) {

            if (!ModelState.IsValid) {
                return BadRequest(new { errormsg = "InvalidRequest." });
            }

            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
            Users targetUser = CustomHelpers.WhoThatAuthUser(authorizationHeader, _context);

            if (targetUser != null) {
                // 일반댓글
                if (postComment.ReplyCid == 0) { 

                    Comments newComment = new Comments {
                        BoardId = postComment.BoardId,
                        NickName = targetUser.NickName,
                        Contents = postComment.Contents
                    };

                    _context.Comments.Add(newComment);
                    await _context.SaveChangesAsync();

                // 답글일 때.
                } else { 

                    Comments? targetComment = await _context.Comments.FindAsync(postComment.ReplyCid);

                    Comments newComment = new Comments {
                        BoardId = postComment.BoardId,
                        ReplyCid = postComment.ReplyCid,
                        Depth = targetComment.Depth + 1,
                        NickName = targetUser.NickName,
                        Contents = postComment.Contents
                    };

                    _context.Comments.Add(newComment);
                    await _context.SaveChangesAsync();
                }
                return Ok(new {msg = "Create Complate." });
            } else {
                return Unauthorized(new { errormsg = "Bad Auth." });
            }
        }

        /// <summary>
        /// (부가기능)댓글 읽기. 해당 게시글의 댓글들만 확인.
        /// </summary>
        /// <param name="boardId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Boards/ReadComments")]
        public IActionResult ReadComments(int boardId) {

            //var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
            //Users targetUser = CustomHelpers.WhoThatAuthUser(authorizationHeader, _context);

            var targetComments = _context.Comments.Where(c => c.BoardId == boardId);
            List<ReadCommentsForm> returnComments = new();

            foreach (var comment in targetComments) {

                string tmpContents;

                if (comment.IsDeleted) {
                    tmpContents = "삭제된 댓글입니다.";
                } else {
                    tmpContents = comment.Contents;
                }


                if (comment.ReplyCid == 0) {

                    ReadCommentsForm tmpComment = new ReadCommentsForm {
                        CommentId = comment.CommentId,
                        BoardId = comment.BoardId,
                        ReplyCid = comment.ReplyCid,
                        Depth = comment.Depth,
                        IsDeleted = comment.IsDeleted,
                        NickName = comment.NickName,
                        Contents = tmpContents,
                        WriteTime = comment.WriteTime,
                        ModifiedTime = comment.ModifiedTime
                    };
                    returnComments.Add(tmpComment);

                } else {

                    ReadCommentsForm parentComment = returnComments.Find(x => x.CommentId == comment.ReplyCid);
                    int idx = returnComments.IndexOf(parentComment);

                    for (int j = idx + 1; j < returnComments.Count; j++) {
                        if (returnComments[j].Depth > parentComment.Depth) {
                            idx++;
                        } else {
                            break;
                        }
                    }

                    ReadCommentsForm tmpComment = new ReadCommentsForm {
                        CommentId = comment.CommentId,
                        BoardId = comment.BoardId,
                        ReplyCid = comment.ReplyCid,
                        Depth = comment.Depth,
                        IsDeleted = comment.IsDeleted,
                        NickName = comment.NickName,
                        Contents = tmpContents,
                        WriteTime = comment.WriteTime,
                        ModifiedTime = comment.ModifiedTime
                    };
                    returnComments.Insert(idx + 1, tmpComment);
                }
            }

            return Ok(returnComments);

        }

        /// <summary>
        /// 댓글 수정
        /// </summary>
        /// <param name="postComment">
        /// <list type="bullet">CommentId : 댓글 Id</list>
        /// <list type="bullet">Contents : 내용</list>
        /// </param>
        /// <returns></returns>
        [HttpPut]
        [Route("api/Boards/UpdateComment")]
        public async Task<IActionResult> UpdateComment([FromBody] UpdateComments postComment) {

            if (!ModelState.IsValid) {
                return BadRequest(new { errormsg = "InvalidRequest." });
            }
            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
            Users targetUser = CustomHelpers.WhoThatAuthUser(authorizationHeader, _context);
            Comments? targetComment = await _context.Comments.FindAsync(postComment.CommentId); //async해서 if문 시간 줄이기?


            if (targetUser != null &&
                targetComment != null &&
                targetUser.NickName == targetComment.NickName &&
                !targetComment.IsDeleted) {

                targetComment.Contents = postComment.Contents;
                targetComment.ModifiedTime = DateTime.Now;

                await _context.SaveChangesAsync();

                return Ok(new {msg = "Update Complate."});

            } else {

                return Unauthorized(new { errormsg = "Bad Auth." });
            }
        }

        /// <summary>
        /// 댓글 삭제
        /// </summary>
        /// <param name="commentId">댓글 Id</param>
        /// <returns></returns>
        [HttpDelete]
        [Route("api/Boards/DeleteComment")]
        public async Task<IActionResult> DeleteComment(int commentId) {

            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
            Users targetUser = CustomHelpers.WhoThatAuthUser(authorizationHeader, _context);
            Comments? targetComment = await _context.Comments.FindAsync(commentId); //async해서 if문 시간 줄이기?

            if (targetUser != null && targetComment != null && targetUser.NickName == targetComment.NickName) {

                targetComment.IsDeleted = true;
                await _context.SaveChangesAsync();

                return Ok(new { msg = "Delete Complate." });

            } else {

                return Unauthorized(new { errormsg = "Bad Auth." });
            }
        }











        // scafold views

        // GET: Boards
        public async Task<IActionResult> Index()
        {
              return _context.Boards != null ? 
                          View(await _context.Boards.ToListAsync()) :
                          Problem("Entity set 'bangsooContext.Boards'  is null.");
        }

        // GET: Boards/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Boards == null)
            {
                return NotFound();
            }

            var boards = await _context.Boards
                .FirstOrDefaultAsync(m => m.BoardId == id);
            if (boards == null)
            {
                return NotFound();
            }

            return View(boards);
        }

        // GET: Boards/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Boards/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BoardId,Id,Title,Contents,ViewCount,WriteTime,ModifiedTime")] Boards boards)
        {
            if (ModelState.IsValid)
            {
                _context.Add(boards);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(boards);
        }

        // GET: Boards/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Boards == null)
            {
                return NotFound();
            }

            var boards = await _context.Boards.FindAsync(id);
            if (boards == null)
            {
                return NotFound();
            }
            return View(boards);
        }

        // POST: Boards/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,Title,Contents,ViewCount,WriteTime,ModifiedTime")] Boards boards)
        {
            if (id != boards.BoardId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(boards);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BoardsExists(boards.BoardId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(boards);
        }

        // GET: Boards/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Boards == null)
            {
                return NotFound();
            }

            var boards = await _context.Boards
                .FirstOrDefaultAsync(m => m.BoardId == id);
            if (boards == null)
            {
                return NotFound();
            }

            return View(boards);
        }

        // POST: Boards/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Boards == null)
            {
                return Problem("Entity set 'bangsooContext.Boards'  is null.");
            }
            var boards = await _context.Boards.FindAsync(id);
            if (boards != null)
            {
                _context.Boards.Remove(boards);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BoardsExists(int id)
        {
          return (_context.Boards?.Any(e => e.BoardId == id)).GetValueOrDefault();
        }

    }
}
