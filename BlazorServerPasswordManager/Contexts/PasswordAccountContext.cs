using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BlazorServerPasswordManager.Contexts;

public partial class PasswordAccountContext : DbContext
{
    public PasswordAccountContext()
    {
    }

    public PasswordAccountContext(DbContextOptions<PasswordAccountContext> options)
        : base(options)
    {
    }

    public virtual DbSet<PasswordmanagerAccount> PasswordmanagerAccounts { get; set; }

    public virtual DbSet<PasswordmanagerUser> PasswordmanagerUsers { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Userrole> Userroles { get; set; }

    public virtual DbSet<Usertoken> Usertokens { get; set; }

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

        modelBuilder.Entity<PasswordmanagerUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("passwordmanager_users_pkey");

            entity.ToTable("passwordmanager_users");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Accessfailedcount).HasColumnName("accessfailedcount");
            entity.Property(e => e.Datelastlogin)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datelastlogin");
            entity.Property(e => e.Datelastlogout)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datelastlogout");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Emailconfirmed)
                .HasColumnType("bit(1)")
                .HasColumnName("emailconfirmed");
            entity.Property(e => e.Firstname).HasColumnName("firstname");
            entity.Property(e => e.Lastname).HasColumnName("lastname");
            entity.Property(e => e.Lockoutenabled)
                .HasColumnType("bit(1)")
                .HasColumnName("lockoutenabled");
            entity.Property(e => e.Lockoutenddateutc).HasColumnName("lockoutenddateutc");
            entity.Property(e => e.Passwordhash).HasColumnName("passwordhash");
            entity.Property(e => e.Salt).HasColumnName("salt");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        modelBuilder.Entity<Userrole>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("userroles");

            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Role).WithMany()
                .HasForeignKey(d => d.Roleid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("userroles_roleid_fkey");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("userroles_userid_fkey");
        });

        modelBuilder.Entity<Usertoken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("usertokens_pkey");

            entity.ToTable("usertokens");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Loginprovider).HasColumnName("loginprovider");
            entity.Property(e => e.Providerkey).HasColumnName("providerkey");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithMany(p => p.Usertokens)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("usertokens_userid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
