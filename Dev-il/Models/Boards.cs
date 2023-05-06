//using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bangsoo.Models {
    public class Boards {
        /*
         * 게시글 DB
         * Id / WriteTime / ModifiedTime / ViewCount / BoardType : 자동생성
         * Username : 유저아이디, Title : 제목, Content : 내용
         */
        [Key, Column(TypeName = "int")]
        public int BoardId { get; set; }


        [ForeignKey("Users")]
        [Required, StringLength(30), Column(TypeName = "nvarchar(30)")]
        public string NickName { get; set; }
        public virtual Users User { get; set; } // Lazy Loading


        [Required, StringLength(127), Column(TypeName = "nvarchar(127)")]
        public string Title { get; set; }


        [Required, Column(TypeName = "nvarchar(MAX)")]
        public string Contents { get; set; }


        // 0:자유게시판 / 1~
        [Column(TypeName = "int")]
        public int BoardType { get; set; } = 0;


        [Column(TypeName = "int")]
        public int ViewCount { get; set; } = 0;


        public bool IsDeleted { get; set; } = false;


        [DataType(DataType.DateTime), Column(TypeName = "datetime")]
        public DateTime WriteTime { get; set; } = DateTime.Now;


        [DataType(DataType.DateTime), Column(TypeName = "datetime")]
        public DateTime ModifiedTime { get; set; } = DateTime.Now;

        public virtual ICollection<Comments> Comments { get; set; } // 보드가 여러개의 댓글을 가질 때.
    }
}
