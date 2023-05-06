using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;
//using System.ComponentModel.DataAnnotations;

namespace bangsoo.Models {
    public class DTOs {
        public class RegisterForm {
            [Required]
            public string UserName { get; set; }
            [Required]
            public string NickName { get; set; }
            [Required]
            public string Password1 { get; set; }
            [Required]
            public string Password2 { get; set; }
        }
        public class LoginForm {
            [Required]
            public string UserName { get; set; }
            [Required]
            public string Password { get; set; }
        }

        public class FindPasswordForm {
            [Required]
            public string UserName { get; set; }
            [Required]
            public string PersonalToken { get; set; }
        }

        public class ChangePasswordForm {
            [Required]
            public string Password1 { get; set; }
            [Required]
            public string Password2 { get; set; }
        }

        public class IsThereForm {
            [Required]
            public string UserOrNick { get; set; }
            [Required]
            public string PostName { get; set; }
        }


        // Recieve Form
        public class CreateBoardForm {
            [Required]
            public string Title { get; set; }
            [Required]
            public string Contents { get; set; }
            [Required]
            public int BoardType { get; set; }
        }

        public class UpdateBoardForm {
            [Required]
            public int BoardId { get; set; }
            [Required]
            public string Title { get; set; }
            [Required]
            public string Contents { get; set; }
        }

        public class CreateComments {
            [Required]
            public int BoardId { get; set; }
            [Required]
            public int ReplyCid { get; set; } // 해당 CID로 조회한 후 그댓글의 Depth + 1
            [Required]
            public string Contents { get; set; }

        }

        public class UpdateComments {
            [Required]
            public int CommentId { get; set; }
            [Required]
            public string Contents { get; set; }
        }



        // Send Form
        public class SimpleBoardsForm {
            public int BoardId { get; set; }
            public string NickName { get; set; }
            public string Title { get; set; }
            public int ViewCount { get; set; }
            public DateTime WriteTime { get; set; }
            public int CommentsCount { get; set; }
            public bool IsDeleted { get; set; }
        }

        public class BoardDetailForm {
            public int BoardId { get; set; }
            public int BoardType { get; set; }
            public string NickName { get; set; }
            public string Title { get; set; }
            public string Contents { get; set; }
            public int ViewCount { get; set; }
            public DateTime WriteTime { get; set; }
            public DateTime ModifiedTime { get; set; }
            public List<ReadCommentsForm> Comments { get; set;}
        }

        public class ReadCommentsForm {
            public int CommentId { get; set; }
            public int ReplyCid { get; set; }
            public int Depth { get; set; }
            public bool IsDeleted { get; set; }
            public int BoardId { get; set; }
            public string NickName { get; set; }
            public string Contents { get; set; }
            public DateTime WriteTime { get; set; }
            public DateTime ModifiedTime { get; set; }
        }

        public class MyPageForm {
            public List<SimpleBoardsForm> Boards { get; set; }
            public List<ReadCommentsForm> Comments { get; set; }
            public string ProfileImageUrl { get; set; }
            public string NickName { get; set; }
        }
    }
}
