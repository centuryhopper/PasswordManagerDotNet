using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PasswordManager.Shared;

namespace PasswordManager.Data;

public partial class PasswordAccountContext : DbContext
{
    public PasswordAccountContext(DbContextOptions<PasswordAccountContext> options)
        : base(options)
    {
    }

    public virtual DbSet<PasswordmanagerAccount> PasswordmanagerAccounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Server=suleiman.db.elephantsql.com;Database=bsqwowlw;User Id=bsqwowlw;Password=YjBJGARHBrBsODfom0W-7W4XxxKkaIot;Port=5432");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("btree_gin")
            .HasPostgresExtension("btree_gist")
            .HasPostgresExtension("citext")
            .HasPostgresExtension("cube")
            .HasPostgresExtension("dblink")
            .HasPostgresExtension("dict_int")
            .HasPostgresExtension("dict_xsyn")
            .HasPostgresExtension("earthdistance")
            .HasPostgresExtension("fuzzystrmatch")
            .HasPostgresExtension("hstore")
            .HasPostgresExtension("intarray")
            .HasPostgresExtension("ltree")
            .HasPostgresExtension("pg_stat_statements")
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("pgrowlocks")
            .HasPostgresExtension("pgstattuple")
            .HasPostgresExtension("tablefunc")
            .HasPostgresExtension("unaccent")
            .HasPostgresExtension("uuid-ossp")
            .HasPostgresExtension("xml2");

        modelBuilder.Entity<PasswordmanagerAccount>(entity =>
        {
            entity.HasKey(e => new { e.Id, e.Userid }).HasName("passwordmanager_accounts_pkey");

            entity.ToTable("passwordmanager_accounts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Userid).HasColumnName("userid");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.LastUpdatedAt).HasColumnName("last_updated_at");
            entity.Property(e => e.Password).HasColumnName("password");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.Username).HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
