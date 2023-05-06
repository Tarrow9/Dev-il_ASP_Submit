using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace bangsoo.Models {
    public class UserRoles : IdentityUserRole<string> {
        /*
        [Key]
        [ForeignKey("Users")]
        public int UserId { get; set; }
        public virtual Users User { get; set; } // Lazy Loading

        [Key]
        [ForeignKey("Roles")]
        public int RoleId { get; set; }
        public virtual Roles Role { get; set; } // Lazy Loading
        */
    }
}
