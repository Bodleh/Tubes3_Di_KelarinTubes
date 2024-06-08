using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace server
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public DbSet<Biodata> Biodata { get; set; }
        public DbSet<SidikJari> SidikJari { get; set; }

        public DbSet<EncryptedBiodata> EncryptedBiodata { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SidikJari>()
                .HasNoKey();
        }
    }
}
