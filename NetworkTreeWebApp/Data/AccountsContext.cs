using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Types;

namespace NetworkTreeWebApp.Data
{
    public partial class AccountsContext : DbContext
    {
        private readonly IConfiguration _config;

        public AccountsContext()
        {
        }

        public AccountsContext(DbContextOptions<AccountsContext> options, IConfiguration config)
            : base(options)
        {
            this._config = config;
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountHierarchy> AccountHierarchies { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_config["ConnectionStrings:dBContext"]);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account");
                entity.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
                entity.HasIndex(e => e.ParentId);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasMany(d => d.Children)
                    .WithOne()
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_Account_Self_ParentId");
                
                entity.HasMany(d => d.Downlinks)
                    .WithOne()
                    .HasForeignKey(d => d.UplinkId)
                    .HasConstraintName("FK_Account_Self_UplinkId");
            });
            modelBuilder.Entity<Account>().HasData(
                    new Account(){ Id = 1, Name = "A", PlacementPreference = 3, ParentId = null, UplinkId = null },
                    new Account(){ Id = 2, Name = "B", PlacementPreference = 2, ParentId = 1, UplinkId = 1 },
                    new Account(){ Id = 3, Name = "C", PlacementPreference = 3, ParentId = 1, UplinkId = 1 },
                    new Account(){ Id = 4, Name = "D", PlacementPreference = 3, ParentId = 2, UplinkId = 1 },
                    new Account(){ Id = 5, Name = "H", PlacementPreference = 1, ParentId = 3, UplinkId = 1 },
                    new Account(){ Id = 6, Name = "K", PlacementPreference = 3, ParentId = 3, UplinkId = 1 },
                    new Account(){ Id = 7, Name = "F", PlacementPreference = 2, ParentId = 2, UplinkId = 1 },
                    new Account(){ Id = 8, Name = "G", PlacementPreference = 3, ParentId = 4, UplinkId = 1 },
                    new Account(){ Id = 9, Name = "V", PlacementPreference = 3, ParentId = 4, UplinkId = 1 },
                    new Account(){ Id = 10, Name = "L", PlacementPreference = 3, ParentId = 6, UplinkId = 1 },
                    new Account(){ Id = 11, Name = "Q", PlacementPreference = 3, ParentId = 7, UplinkId = 2 },
                    new Account(){ Id = 12, Name = "X", PlacementPreference = 1, ParentId = 11, UplinkId = 2 },
                    new Account(){ Id = 13, Name = "Y", PlacementPreference = 2, ParentId = 11, UplinkId = 2 }
            );

            modelBuilder.Entity<AccountHierarchy>(entity =>
            {
                entity.ToTable("AccountHierarchy");
                entity.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
                entity.HasIndex(e => e.ParentId);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasMany(d => d.Children)
                    .WithOne()
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_AccountHierarchy_Self_ParentId");
                
                entity.HasMany(d => d.Downlinks)
                    .WithOne()
                    .HasForeignKey(d => d.UplinkId)
                    .HasConstraintName("FK_AccountHierarchy_Self_UplinkId");
            });

            modelBuilder.Entity<AccountHierarchy>().HasData(
                    new AccountHierarchy(){ Id = 1, Name = "A", PlacementPreference = 3, ParentId = null, UplinkId = null, LevelPath = "/" }
                    // new AccountHierarchy(){ Id = 2, Name = "B", PlacementPreference = 2, ParentId = 1, UplinkId = 1, LevelPath = "/1" },
                    // new AccountHierarchy(){ Id = 3, Name = "C", PlacementPreference = 3, ParentId = 1, UplinkId = 1, LevelPath = "/2" },
                    // new AccountHierarchy(){ Id = 4, Name = "D", PlacementPreference = 3, ParentId = 2, UplinkId = 1, LevelPath = "/1/1" },
                    // new AccountHierarchy(){ Id = 5, Name = "H", PlacementPreference = 1, ParentId = 3, UplinkId = 1, LevelPath = "/2/1" },
                    // new AccountHierarchy(){ Id = 6, Name = "K", PlacementPreference = 3, ParentId = 3, UplinkId = 1, LevelPath = "/2/2" },
                    // new AccountHierarchy(){ Id = 7, Name = "F", PlacementPreference = 2, ParentId = 2, UplinkId = 1, LevelPath = "/1/2" },
                    // new AccountHierarchy(){ Id = 8, Name = "G", PlacementPreference = 3, ParentId = 4, UplinkId = 1, LevelPath = "/1/1/1" },
                    // new AccountHierarchy(){ Id = 9, Name = "V", PlacementPreference = 3, ParentId = 4, UplinkId = 1, LevelPath = "/1/1/2" },
                    // new AccountHierarchy(){ Id = 10, Name = "L", PlacementPreference = 3, ParentId = 6, UplinkId = 1, LevelPath = "/2/2/1" }
            );

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
