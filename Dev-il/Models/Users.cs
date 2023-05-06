using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bangsoo.Models {
    public class Users : IdentityUser {

        [Required, StringLength(30), Column(TypeName = "nvarchar(30)")]
        public string NickName { get; set; }

        [StringLength(128), Column(TypeName = "nvarchar(256)")]
        public string? ProfileImageUrl { get; set; }  = "https://localhost:32768/Images/default.jpg"; // 기본 URL 할당하기

        [StringLength(8), Column(TypeName = "nchar(8)")]
        public string PersonalToken { get; set;}

        public bool IsDeleted { get; set; } = false;

        public virtual ICollection<Boards> Boards { get; set; } // 유저가 여러개의 글을 가질 때.
        public virtual ICollection<Comments> Comments { get; set; } // 유저가 여러개의 글을 가질 때.
        // public virtual Boards Boards { get; set; } // 유저가 단 하나의 글을 가질 때.
    }
}
