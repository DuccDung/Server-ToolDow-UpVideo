using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Client_Get_Token.Models;

public partial class InformationMeetingContext : DbContext
{
    public InformationMeetingContext()
    {
    }

    public InformationMeetingContext(DbContextOptions<InformationMeetingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<GoogleToken> GoogleTokens { get; set; }

    public virtual DbSet<Meeting> Meetings { get; set; }

    public virtual DbSet<RecordingFile> RecordingFiles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=ADMIN-PC\\MSSQLSERVER1;Initial Catalog=InformationMeeting;User ID=sa;Password=Dung@123;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GoogleToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GoogleTo__3214EC072880BADA");

            entity.ToTable("GoogleToken");

            entity.Property(e => e.IssuedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Scope).HasMaxLength(500);
            entity.Property(e => e.TokenType).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasMaxLength(100);
        });

        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.HasKey(e => e.Uuid).HasName("PK__Meeting__BDA103F59A105013");

            entity.ToTable("Meeting");

            entity.Property(e => e.Uuid).HasMaxLength(100);
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.Topic).HasMaxLength(255);
        });

        modelBuilder.Entity<RecordingFile>(entity =>
        {
            entity.HasKey(e => e.FileId).HasName("PK__Recordin__6F0F98BF47C1F6BE");

            entity.ToTable("RecordingFile");

            entity.Property(e => e.FileId).HasMaxLength(100);
            entity.Property(e => e.DownloadedAt).HasColumnType("datetime");
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FileType).HasMaxLength(50);
            entity.Property(e => e.MeetingUuid).HasMaxLength(100);

            entity.HasOne(d => d.MeetingUu).WithMany(p => p.RecordingFiles)
                .HasForeignKey(d => d.MeetingUuid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_RecordingFile_Meeting");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
