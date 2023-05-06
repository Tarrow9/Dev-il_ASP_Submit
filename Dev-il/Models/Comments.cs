using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bangsoo.Models {
    public class Comments {
        [Key, Column(TypeName = "int")]
        public int CommentId { get; set; }

        public int ReplyCid { get; set; } = 0; // 0이면 답글 아님

        public int Depth { get; set; } = 0; // 0이면 일반답글

        public bool IsDeleted { get; set; } = false;


        [ForeignKey("Boards")]
        [Required, Column(TypeName = "int")]
        public int BoardId { get; set; }
        public virtual Boards Board { get; set; } // Lazy Loading, 하나의 댓글은 하나의 보드ID를 가짐

        [ForeignKey("Users")]
        [Required, StringLength(30), Column(TypeName = "nvarchar(30)")]
        public string NickName { get; set; }
        public virtual Users User { get; set; } // Lazy Loading, 하나의 댓글은 하나의 유저ID를 가짐






        [Required, Column(TypeName = "nvarchar(256)")]
        public string Contents { get; set; }


        [DataType(DataType.DateTime), Column(TypeName = "datetime")]
        public DateTime WriteTime { get; set; } = DateTime.Now;


        [DataType(DataType.DateTime), Column(TypeName = "datetime")]
        public DateTime ModifiedTime { get; set; } = DateTime.Now;
    }
}
