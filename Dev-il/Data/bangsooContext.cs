using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using bangsoo.Models;
using NuGet.DependencyResolver;
using Microsoft.AspNetCore.Identity;

namespace bangsoo.Data
{
    public class bangsooContext : DbContext
    {
        public bangsooContext (DbContextOptions<bangsooContext> options)
            : base(options)
        {
        }
        public DbSet<Boards> Boards { get; set; } //분리필요
        public DbSet<Comments> Comments { get; set; }
        public DbSet<Users> Users { get; set; } = default!;
        public DbSet<Roles> Roles { get; set; }

        // dbo.UserRoles 이름으로 identityUserRole DbSet사용
        public DbSet<IdentityUserRole<string>> UserRoles { get; set; } 


        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Users>()
                .HasIndex(u => u.NickName)
                .IsUnique();

            // 계속해서 늘어나는 테이블이므로 데이터가 많아질 수록 성능저하됨.
            // modelBuilder.Entity<Boards>()
            //     .HasIndex(u => u.BoardType);

            modelBuilder.Entity<IdentityUserRole<string>>()
                .HasKey(u => new { u.UserId, u.RoleId });

            modelBuilder.Entity<Roles>().HasData(
                new Roles { Id = "1", Name = "User", NormalizedName = "USER" },
                new Roles { Id = "2", Name = "Admin", NormalizedName = "ADMIN" }
            );

            modelBuilder.Entity<Comments>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.NickName)
                .HasPrincipalKey(u => u.NickName)
                .OnDelete(DeleteBehavior.NoAction); // OnDelete를 NoAction으로 설정

            modelBuilder.Entity<Comments>()
                .HasOne(c => c.Board)
                .WithMany(b => b.Comments)
                .HasForeignKey(c => c.BoardId);//.OnDelete(DeleteBehavior.NoAction); // OnDelete를 NoAction으로 설정

            modelBuilder.Entity<Boards>()
                .HasOne(u => u.User)
                .WithMany(b => b.Boards)
                .HasForeignKey(b => b.NickName)
                .HasPrincipalKey(u => u.NickName)
                .OnDelete(DeleteBehavior.NoAction);
            
            // modelBuilder.Entity<Comments>()
            //     .HasOne(u => u.User)
            //     .WithMany(c => c.Comments)
            //     .HasForeignKey(c => c.Id)
            //     .OnDelete(DeleteBehavior.Cascade);
            // 
            // modelBuilder.Entity<Comments>()
            //     .HasOne(b => b.Board)
            //     .WithMany(c => c.Comments)
            //     .HasForeignKey(c => c.BoardId)
            //     .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
