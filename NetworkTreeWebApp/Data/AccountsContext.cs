using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

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

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.Children)
                    .HasForeignKey(d => d.ParentId)
                    .HasConstraintName("FK_Account_Self_ParentId");
            });
            modelBuilder.Entity<Account>().HasData(
                new List<Account>()
                {
                    new Account(){ Id = 1, Name = "A", PlacementPreference = 3, ParentId = null },
                    new Account(){ Id = 2, Name = "B", PlacementPreference = 3, ParentId = 1 },
                    new Account(){ Id = 3, Name = "C", PlacementPreference = 3, ParentId = 1 },
                    new Account(){ Id = 4, Name = "D", PlacementPreference = 3, ParentId = 2 },
                    new Account(){ Id = 5, Name = "H", PlacementPreference = 3, ParentId = 3 },
                    new Account(){ Id = 6, Name = "K", PlacementPreference = 3, ParentId = 3 },
                    new Account(){ Id = 7, Name = "F", PlacementPreference = 3, ParentId = 2 },
                    new Account(){ Id = 8, Name = "G", PlacementPreference = 3, ParentId = 4 },
                    new Account(){ Id = 9, Name = "V", PlacementPreference = 3, ParentId = 4 },
                    new Account(){ Id = 10, Name = "L", PlacementPreference = 3, ParentId = 6 },
                }
            );

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
