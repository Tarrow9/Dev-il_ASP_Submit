using bangsoo.Data;
using bangsoo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Security.Cryptography;
using static bangsoo.Models.DTOs;
using NuGet.Common;

using Microsoft.AspNetCore.Http;


namespace bangsoo.Controllers {

    /// <summary>
    /// Personal token 생성기
    /// </summary>
    /// <param name="salt_Eight">DB에 저장된 Salt값</param>
    /// <param name="MakingSalt_Eight">DB에 저장될 Salt값 만드는 함수</
    /// <param name="SetSalt_Eight">지정한 문자열로 salt_Eight을 설정하는 함수</param
    /// <param name="MakingPersonalToken">Salt를 이용해 Token 생성하는 함수</param>
    /// <returns>System.IdentityModel.Tokens.Jwt.JwtSecurityToken</returns>
    internal class CustomPersonalToken {
        public string salt_Eight = ""; //DB저장용

        public void MakingSalt_Eight() {
            Random rand = new Random();
            string str = "****Censored****";
            for (int i = 0; i < 8; i++) {
                salt_Eight += str[rand.Next(str.Length)];
            }
        }
        public void SetSalt_Eight(string str) {
            salt_Eight = str;
        }
        public string? MakingPersonalToken(string nickName) {
            if (salt_Eight == "") {
                return null;
            }
            byte[] bytearray = Encoding.ASCII.GetBytes(nickName + salt_Eight + "****Censored****");
            SHA256 sha = SHA256.Create();
            byte[] hashVal = sha.ComputeHash(bytearray, 0, bytearray.Length);//personalToken 발급용

            var sBuilder = new StringBuilder();
            for (int i = 0; i < hashVal.Length; i++) {
                sBuilder.Append(hashVal[i].ToString("x2"));
            }
            string returnstr = sBuilder.ToString();

            return returnstr;
        }
    }

    public class UsersController : Controller {
        // DI
        private bangsooContext _context;
        private UserManager<Users> _userManager;
        // private readonly SignInManager<Users> _signInManager;
        public UsersController(bangsooContext context, UserManager<Users> userManager) {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// JWT token 생성기
        /// </summary>
        /// <param name="targetUserNickName">해당하는 사람의 닉네임</param>
        /// <returns>System.IdentityModel.Tokens.Jwt.JwtSecurityToken</returns>
        private async Task<JwtSecurityToken> GenerateToken(string targetUserNickName) {

            var targetUser = _context.Users.FirstOrDefault(userTable => userTable.NickName == targetUserNickName);
            var roles = await _userManager.GetRolesAsync(targetUser);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, targetUserNickName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                // Authrize[Roles="User"] 할때 확인하는 부분
                new Claim("role", roles[0]),
            };

            var token = new JwtSecurityToken(
                issuer: "dev-il.kr",
                audience: "dev-il.kr",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("****Censored****")), SecurityAlgorithms.HmacSha256)
            );
            return token;
        }

        /// <summary>
        /// Password 변경시 사용하는 토큰 생성기
        /// </summary>
        /// <param name="claims">클레임 커스터마이징 필요</param>
        /// <returns></returns>
        private JwtSecurityToken GenerateToken(Claim[] claims) {
            var token = new JwtSecurityToken(
                issuer: "dev-il.kr",
                audience: "dev-il.kr",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(3),
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("****Censored****")), SecurityAlgorithms.HmacSha256)
            );
            return token;
        }
        




        /// <summary>
        /// 회원가입
        /// </summary>
        /// <param name="postNewUser">POST로 받은 값들.
        ///     <list type="bullet">ID = UserName</list>
        ///     <list type="bullet">NickName</list>
        ///     <list type="bullet">Password 1,2</list>
        /// </param>
        /// <returns>JWToken / 유효기간 / 완료메시지</returns>
        [HttpPost]
        [Route("api/Users/RegisterUser")]
        [AllowAnonymous]
        // cshtml from tag에 @Html.AntiForgeryToken() 해주면 됨. Django의 {% csrf_token %} 개념
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterForm postNewUser) {
            // Required 필드 유효성 검사
            if (!ModelState.IsValid) {
                return BadRequest(new { errormsg = "InvalidRequest." });
            }


            if (postNewUser.Password1 != postNewUser.Password2) {
                return BadRequest(new { errormsg = "NotMatchPasswords." });
            }
            if (postNewUser.UserName == postNewUser.Password1) {
                return BadRequest(new { errormsg = "UserNameMatchPassword." });
            }
            var existUser = _context.Users.Where(userTable => userTable.NickName == postNewUser.NickName).FirstOrDefault();
            if (existUser != null) {
                return BadRequest(new { errormsg = "NickNameDuplicated." });
            }

            // personalToken 저장 대신 salt값 저장, 그리고 히든키는 서버에 하드코딩.
            CustomPersonalToken PTInstance = new CustomPersonalToken();
            PTInstance.MakingSalt_Eight();

            var NewUser = new Users {
                UserName = postNewUser.UserName,
                NickName = postNewUser.NickName,
                PersonalToken = PTInstance.salt_Eight
            };

            var result = await _userManager.CreateAsync(NewUser, postNewUser.Password1);
            if (!result.Succeeded) {
                /* "Codes"
                 * PasswordTooShort = len need above 5
                 * PasswordRequiresDigit = need Digit
                 * PasswordRequiresNonAlphanumeric = need alphabet and numeric
                 * PasswordRequiresLower, PasswordRequiresUpper = need Lower and Upper
                 * UserName == Password??
                 */
                System.Diagnostics.Trace.WriteLine(result);
                return BadRequest(new { errormsg = result.Errors.ToArray()[0].Code });
            }

            await _userManager.AddToRoleAsync(NewUser, "User");

            var token = await GenerateToken(NewUser.NickName);

            return Ok(new {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                message = "Register and Login Complate",
                personalToken = PTInstance.MakingPersonalToken(NewUser.NickName)
            });
        }

        /// <summary>
        /// 아이디/닉네임 중복확인
        /// </summary>
        /// <param name="post">검증받을 값과 문자열
        ///     <list type="bullet">UserOrNick : UserName / NickName 중 선택</list>
        ///     <list type="bullet">postName : 검증문자열</list>
        /// </param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Users/IsThereName")]
        [AllowAnonymous]
        public IActionResult IsThereName([FromBody] IsThereForm post) {

            if (!ModelState.IsValid) {
                return BadRequest(new { errormsg = "InvalidRequest." });
            }


            Users? existUser;
            if (post.UserOrNick == "UserName") {
                existUser = _context.Users.Where(userTable => userTable.UserName == post.PostName).FirstOrDefault();

            } else if (post.UserOrNick == "NickName") {
                existUser = _context.Users.Where(userTable => userTable.NickName == post.PostName).FirstOrDefault();

            } else {
                return BadRequest(new { errormsg = "Choose correct target, UserName Or NickName." });
            }


            if (existUser != null) {

                return BadRequest(new { errormsg = "NameDuplicated." });
            }
            return Ok(new { msg = "Usable Name." });
        }

        /// <summary>
        /// 로그인
        /// </summary>
        /// <param name="postUser">POST로 받은 값들.
        ///     <list type="bullet">ID = UserName</list>
        ///     <list type="bullet">Password</list>
        /// </param>
        /// <returns>JWToken / 유효기간 / 완료메시지</returns>
        [HttpPost]
        [Route("api/Users/Login")]
        [AllowAnonymous]
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromBody] LoginForm postUser) {
            // Required 필드 유효성 검사
            if (!ModelState.IsValid) {
                return BadRequest(new { errormsg = "InvalidRequest." });
            }

            //var loginUser = _context.Users.Where(userTable => userTable.UserName == postUser.UserName).FirstOrDefault();
            var loginUser = _context.Users.FirstOrDefault(userTable => userTable.UserName == postUser.UserName);
            if (loginUser == null || loginUser.IsDeleted) {
                return Unauthorized(new { msg = "Can't Find ID." });
            }

            bool isCorrect = await _userManager.CheckPasswordAsync(loginUser, postUser.Password);
            if (isCorrect) {

                var token = await GenerateToken(loginUser.NickName);



                // cookie setting
                var cookieOptions = new CookieOptions {
                    Expires = DateTime.Now.AddHours(1), // 쿠키 만료 시간 설정 (예: 1시간 후)
                    Secure = true, // HTTPS 연결에서만 쿠키 전송 설정
                    HttpOnly = true // JavaScript에서 쿠키에 접근하지 못하도록 설정
                };

                Response.Cookies.Append("AccessToken", "MyValue", cookieOptions);

                // 쿠키 읽기
                //var myCookieValue = Request.Cookies["AccessToken"];
                // 쿠키 삭제
                //Response.Cookies.Delete("MyCookie");



                var sendToken = new JwtSecurityTokenHandler().WriteToken(token);

                Response.Cookies.Append("AccessToken", sendToken, cookieOptions);

                return Ok(new {
                    token = sendToken,
                    expiration = token.ValidTo,
                    msg = "Login Complate"
                });

            } else {

                return Unauthorized(new { msg = "Password Failed." });
            }
            
        }
        // Logout : Front에서 쿠키만 지우기

        /// <summary>
        /// 유저 삭제
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Users/WithdrawalUser")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> WithdrawalUser() {

            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
            Users targetUser = CustomHelpers.WhoThatAuthUser(authorizationHeader, _context);

            if (targetUser != null) {

                targetUser.IsDeleted = true;
                await _userManager.UpdateAsync(targetUser);

                return Ok(new { msg = "Withdrawal User Complate." });

            } else {

                return Unauthorized(new { errormsg = "Bad Auth." });
            }
        }





        /// <summary>
        /// 비밀번호 변경 토큰 얻기
        /// </summary>
        /// <param name="postUser">
        ///     <list type="bullet">ID = UserName(닉네임을 모름)</list>
        ///     <list type="bullet">PersonalToken</list>
        /// </param>
        /// <returns>Ok(new {PasswordChangingToken=JWT}) : JWT that added Claim</returns>
        [HttpPost]
        [Route("api/Users/FindPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> FindPassword([FromBody] FindPasswordForm postUser) {
            // Required 필드 유효성 검사
            if (!ModelState.IsValid) {
                return BadRequest(new { errormsg = "InvalidRequest." });
            }

            var findUser = _context.Users.FirstOrDefault(userTable => userTable.UserName == postUser.UserName);
            string tokenForCheck = "";

            if (findUser == null) {
                return Unauthorized(new { errormsg = "Can't Find UserName." });

            } else {
                // 퍼스널토큰 연산
                CustomPersonalToken PTInstance = new();
                PTInstance.SetSalt_Eight(findUser.PersonalToken);
                tokenForCheck = PTInstance.MakingPersonalToken(findUser.NickName);

                if (postUser.PersonalToken == tokenForCheck) {

                    var roles = await _userManager.GetRolesAsync(findUser);

                    var claims = new[] {
                        new Claim(JwtRegisteredClaimNames.Sub, findUser.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        // Authrize[Roles="User"] 할때 확인하는 부분
                        new Claim("role", roles[0]),
                        new Claim("PasswordChanging", "True")
                    };

                    var token = GenerateToken(claims);
                    return Ok(new { 
                        PasswordChangingToken = new JwtSecurityTokenHandler().WriteToken(token)
                    });
                } else {
                    return Unauthorized(new { errormsg = "Not Matched Personal Token." });
                }
            }
        }
        // testcase : test1 / 9c078e973ad617059d23b8e138e33fa30b13c972934eba8508c8d8bf6ee4741c

        /// <summary>
        /// personal token으로 검증된 이용자가 해당 유저의 비밀번호 변경.
        /// </summary>
        /// <param name="newPassword">
        ///     <list type="bullet">Password1</list>
        ///     <list type="bullet">Password2</list>
        /// </param>
        /// <returns>Ok</returns>
        [HttpPost]
        [Route("api/Users/ChangePassword")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult ChangePassword([FromBody] ChangePasswordForm newPassword) {
            // Required 필드 유효성 검사
            if (!ModelState.IsValid) {
                return BadRequest(new { errormsg = "InvalidRequest." });
            }

            if (newPassword.Password1 != newPassword.Password2) {
                return BadRequest(new { errormsg = "Not Matched Passwords." });
            }

            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (authorizationHeader != null && authorizationHeader.StartsWith("Bearer ")) {
                // token 처리
                var token = authorizationHeader.Substring("Bearer ".Length);
                var tokenHandler = new JwtSecurityTokenHandler();
                var claims = tokenHandler.ReadJwtToken(token).Claims;
                var passwordChangingClaim = claims.FirstOrDefault(claim => claim.Type == "PasswordChanging");

                if (passwordChangingClaim != null && passwordChangingClaim.Value == "True") {
                    // 변경하고자 하는 유저 검색
                    var targetUserName = claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value;
                    var targetUser = _context.Users.First(userTable => userTable.UserName == targetUserName);
                    // 비밀번호 없앤 후 변경.
                    _userManager.RemovePasswordAsync(targetUser);
                    _userManager.AddPasswordAsync(targetUser, newPassword.Password1);
                    return Ok(new { msg = "Password Changed." });
                }
            }
            return Unauthorized(new { errormsg = "Unauthorized." });
        }


        

        private const string IMAGE_PATH = @"/app/wwwroot/images";
        private const string DEFAULT_IMAGE_PATH = "https://localhost:32768/Images/default.jpg";
        /// <summary>
        /// 이미지 업로드하고 User에 프로필경로 저장
        /// <list type="bullet">input type = "file" name = "imageFile" id="image-file"</list>
        /// <list type="bullet">button submit</list>
        /// <list type="bullet">header 'Content-Type': 'multipart/form-data'</list>
        /// <list type="bullet">body: formData</list>
        /// </summary>
        [HttpPost]
        [Route("api/Users/UploadProfileImage")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> UploadProfileImage(IFormFile imageFile) {
            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
            Users targetUser = CustomHelpers.WhoThatAuthUser(authorizationHeader, _context);

            if (targetUser == null) {
                return Unauthorized(new { errormsg = "Bad Auth." });
            }

            if (imageFile == null || imageFile.Length == 0) {
                return BadRequest(new { errormsg = "Invalid file." });
            }

            // 이미지 파일의 확장자를 검사
            var extension = Path.GetExtension(imageFile.FileName);
            if (!extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) &&
                !extension.Equals(".png", StringComparison.OrdinalIgnoreCase)) {
                return BadRequest(new { errormsg = "Invalid file type." });
            }

            // 이미지 파일을 파일시스템에 저장
            var filename = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(IMAGE_PATH, filename);
            
            using (var stream = new FileStream(filePath, FileMode.Create)) {
                await imageFile.CopyToAsync(stream);
            }

            // 이미지파일 URL (도메인혹은 변동되는 url때문에 Scheme/Host사용)
            // 정적파일 wwwroot 경로기 때문에 접근이 가능함.
            var imageUrl = $"{Request.Scheme}://{Request.Host}/Images/{filename}";
            targetUser.ProfileImageUrl = imageUrl;
            await _userManager.UpdateAsync(targetUser);
            
            return Ok(new { imageUrl }); // test
        }

        [HttpGet]
        [Route("api/Users/InitProfileImage")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> InitProfileImage() {
            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
            Users targetUser = CustomHelpers.WhoThatAuthUser(authorizationHeader, _context);

            if (targetUser == null) {
                return Unauthorized(new { errormsg = "Bad Auth." });
            }

            targetUser.ProfileImageUrl = DEFAULT_IMAGE_PATH;
            await _userManager.UpdateAsync(targetUser);

            return Ok(new { msg = "Init Profile image Complate." }); // test

        }

        /// <summary>
        /// Form Html for Upload Test
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("api/Users/UploadProfileImage")]
        public IActionResult UploadProfileImage() {
            return View();
        }

        // 페이지네이션 필요?????
        /// <summary>
        /// 내정보 보기
        /// </summary>
        /// <returns>유저가 작성한 게시글/댓글, 프로필사진, 닉네임</returns>
        [HttpGet]
        [Route("api/Users/MyPage")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public IActionResult MyPage() {

            var authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
            Users targetUser = CustomHelpers.WhoThatAuthUser(authorizationHeader, _context, "BoardsAndComments");
            if (targetUser == null) {
                return Unauthorized(new { errormsg = "Bad Auth." });
            }

            MyPageForm returnform = new();
            List<SimpleBoardsForm> tmpBoardList = new();
            List<ReadCommentsForm> tmpCommentList = new();

            // 인스턴스 계속 생산 -> 메모리 낭비 막을 수 있나?
            // => 어차피 List크기만큼 메모리 늘어나기 때문에 참조형으로 넣어도 상관 없음.
            SimpleBoardsForm tmpBoard; 
            foreach (var iterBoard in targetUser.Boards) {

                tmpBoard = new();
                tmpBoard.BoardId = iterBoard.BoardId;
                tmpBoard.NickName = iterBoard.NickName;
                tmpBoard.Title = iterBoard.Title;
                tmpBoard.ViewCount = iterBoard.ViewCount;
                tmpBoard.WriteTime = iterBoard.WriteTime;
                tmpBoard.CommentsCount = iterBoard.Comments.Count;
                tmpBoard.IsDeleted = iterBoard.IsDeleted;
                tmpBoardList.Add(tmpBoard);
            }

            ReadCommentsForm tmpComment;
            foreach (var iterComment in targetUser.Comments) {
                if (iterComment.ReplyCid != 0) {

                    Comments parentComment = _context.Comments.Find(iterComment.ReplyCid);

                    if (targetUser.Comments.Contains(parentComment)) {

                        int cmpCid = parentComment.CommentId;
                        ReadCommentsForm indexingComment = tmpCommentList.Find(x => x.CommentId == cmpCid);

                        tmpComment = new();
                        tmpComment.BoardId = iterComment.BoardId; // 해당 게시글로 이동할 때 사용
                        tmpComment.CommentId = iterComment.CommentId; // 
                        tmpComment.ReplyCid = iterComment.ReplyCid;
                        tmpComment.Depth = iterComment.Depth;
                        tmpComment.IsDeleted = iterComment.IsDeleted;
                        tmpComment.NickName = iterComment.NickName;
                        tmpComment.Contents = iterComment.Contents;
                        tmpComment.WriteTime = iterComment.WriteTime;
                        tmpComment.ModifiedTime = iterComment.ModifiedTime;

                        int idx = tmpCommentList.IndexOf(indexingComment);
                        tmpCommentList.Insert(idx+1, tmpComment);

                    } else {

                        tmpComment = new();
                        tmpComment.BoardId = parentComment.BoardId; // 해당 게시글로 이동할 때 사용
                        tmpComment.CommentId = parentComment.CommentId; // 
                        tmpComment.ReplyCid = parentComment.ReplyCid;
                        tmpComment.Depth = parentComment.Depth;
                        tmpComment.IsDeleted = parentComment.IsDeleted;
                        tmpComment.NickName = parentComment.NickName;
                        tmpComment.Contents = parentComment.Contents;
                        tmpComment.WriteTime = parentComment.WriteTime;
                        tmpComment.ModifiedTime = parentComment.ModifiedTime;
                        tmpCommentList.Add(tmpComment);

                        tmpComment = new();
                        tmpComment.BoardId = iterComment.BoardId; // 해당 게시글로 이동할 때 사용
                        tmpComment.CommentId = iterComment.CommentId; // 
                        tmpComment.ReplyCid = iterComment.ReplyCid;
                        tmpComment.Depth = iterComment.Depth;
                        tmpComment.IsDeleted = iterComment.IsDeleted;
                        tmpComment.NickName = iterComment.NickName;
                        tmpComment.Contents = iterComment.Contents;
                        tmpComment.WriteTime = iterComment.WriteTime;
                        tmpComment.ModifiedTime = iterComment.ModifiedTime;
                        tmpCommentList.Add(tmpComment);
                    }

                } else {
                    tmpComment = new();
                    tmpComment.BoardId = iterComment.BoardId; // 해당 게시글로 이동할 때 사용
                    tmpComment.CommentId = iterComment.CommentId; // 
                    tmpComment.ReplyCid = iterComment.ReplyCid;
                    tmpComment.Depth = iterComment.Depth;
                    tmpComment.IsDeleted = iterComment.IsDeleted;
                    tmpComment.NickName = iterComment.NickName;
                    tmpComment.Contents = iterComment.Contents;
                    tmpComment.WriteTime = iterComment.WriteTime;
                    tmpComment.ModifiedTime = iterComment.ModifiedTime;
                    tmpCommentList.Add(tmpComment);

                }

            }

            returnform.Boards = tmpBoardList;
            returnform.Comments = tmpCommentList;
            returnform.ProfileImageUrl = targetUser.ProfileImageUrl;
            returnform.NickName = targetUser.NickName;

            return Ok(returnform);
        }




        /*
        /// <summary>
        /// Test Logic
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "User")] // jwt => 401, role => 403
        [HttpGet]
        public IActionResult GetUsers() {
            var _users = _context.Users;
            return Ok(_users);
        }
        */
    }
}
