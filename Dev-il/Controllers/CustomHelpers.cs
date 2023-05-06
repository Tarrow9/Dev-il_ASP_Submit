using bangsoo.Data;
using bangsoo.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace bangsoo.Controllers {
    public class CustomHelpers {
        /// <summary>
        /// JWT토큰에서 NickName을 통해 유저 찾는 함수
        /// </summary>
        /// <param name="authHeader">(String) 헤더의 Bearer 포함한 JWT토큰</param>
        /// <returns>Users targetUser</returns>
        static public Users WhoThatAuthUser(string authHeader, bangsooContext _context, string hasWhats = "else") {
            if (authHeader != null && authHeader.StartsWith("Bearer ")) { //null 에러때문
                // token 처리
                var token = authHeader.Substring("Bearer ".Length);
                var tokenHandler = new JwtSecurityTokenHandler();
                var claims = tokenHandler.ReadJwtToken(token).Claims;
                var targetNickName = claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value;

                if (hasWhats == "Boards") {
                    return _context.Users
                        .Include(u => u.Boards)
                        .First(userTable => userTable.NickName == targetNickName);

                } else if (hasWhats == "Comments") {
                    return _context.Users
                        .Include(u => u.Comments)
                        .First(userTable => userTable.NickName == targetNickName);

                } else if (hasWhats == "BoardsAndComments") {
                    return _context.Users
                        .Include(u => u.Boards)
                        .Include(u => u.Comments)
                        .First(userTable => userTable.NickName == targetNickName);

                } else {
                    return _context.Users.First(userTable => userTable.NickName == targetNickName);
                }
            } else {
                return null;
            }
        }
    }
}
