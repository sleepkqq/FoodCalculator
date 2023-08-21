using Microsoft.EntityFrameworkCore;
using Testing.Models;

namespace Testing.Data;

public partial class Rp2023StudentRandContext : DbContext
{
    public Rp2023StudentRandContext()
    {
    }

    public Rp2023StudentRandContext(DbContextOptions<Rp2023StudentRandContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdultDiary> AdultDiaries { get; set; }

    public virtual DbSet<ChildDiary> ChildDiaries { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer(new ConnectionString().connectionString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdultDiary>(entity =>
        {
            entity.HasKey(e => e.ObjectId);

            entity.ToTable("AdultDiary");

            entity.HasIndex(e => e.Adult, "IDX_AdultDiary_Adult");

            entity.Property(e => e.ObjectId)
                .ValueGeneratedNever()
                .HasColumnName("ObjectID");
            entity.Property(e => e.IR6v65).HasColumnName("I_R6V6_5");
            entity.Property(e => e.IR6v66).HasColumnName("I_R6V6_6");
        });

        modelBuilder.Entity<ChildDiary>(entity =>
        {
            entity.HasKey(e => e.ObjectId);

            entity.ToTable("ChildDiary");

            entity.HasIndex(e => e.Child, "IDX_ChildDiary_Child");

            entity.Property(e => e.ObjectId)
                .ValueGeneratedNever()
                .HasColumnName("ObjectID");
            entity.Property(e => e.CR6v45).HasColumnName("C_R6V4_5");
            entity.Property(e => e.CR6v46).HasColumnName("C_R6V4_6");
        });
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
