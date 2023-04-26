using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Battleship_APIs.Models;

public partial class BattleshipDbContext : DbContext
{
    public BattleshipDbContext()
    {
    }

    public BattleshipDbContext(DbContextOptions<BattleshipDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cell> Cells { get; set; }

    public virtual DbSet<Grid> Grids { get; set; }

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<Ship> Ships { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=tcp:battleship.database.windows.net,1433;Initial Catalog=BattleshipDB;Persist Security Info=False;User ID=BattleshipDB;Password=databaseBattleship0;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cell>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Cell");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.GridId).HasColumnName("GridID");
            entity.Property(e => e.ShipId).HasColumnName("ShipID");
            entity.Property(e => e.Xaxis).HasColumnName("XAxis");
            entity.Property(e => e.Yaxis).HasColumnName("YAxis");

            entity.HasOne(d => d.Ship).WithMany(p => p.Cells)
                .HasForeignKey(d => d.ShipId)
                .HasConstraintName("FK_Cells_Ships");
        });

        modelBuilder.Entity<Grid>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Grid");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.PlayerId).HasColumnName("PlayerID");

            entity.HasOne(d => d.Player).WithMany(p => p.Grids)
                .HasForeignKey(d => d.PlayerId)
                .HasConstraintName("FK_Grid_Player1");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Player");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.ShotGridId).HasColumnName("ShotGridID");
            entity.Property(e => e.UserGridId).HasColumnName("UserGridID");

            entity.HasOne(d => d.ShotGrid).WithMany(p => p.PlayerShotGrids)
                .HasForeignKey(d => d.ShotGridId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Player_Grid");

            entity.HasOne(d => d.UserGrid).WithMany(p => p.PlayerUserGrids)
                .HasForeignKey(d => d.UserGridId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Player_Grid1");
        });

        modelBuilder.Entity<Ship>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Ship");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Hp).HasColumnName("HP");
            entity.Property(e => e.PlayerId).HasColumnName("PlayerID");

            entity.HasOne(d => d.Player).WithMany(p => p.Ships)
                .HasForeignKey(d => d.PlayerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Ship_Player");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
