using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Server_ToolDow_UpVideo.Models;

public partial class InformationMeetingContext : DbContext
{
    public InformationMeetingContext()
    {
    }

    public InformationMeetingContext(DbContextOptions<InformationMeetingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ZoomRecording> ZoomRecordings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Data Source=ADMIN-PC\\MSSQLSERVER1;Initial Catalog=InformationMeeting;User ID=sa;Password=Dung@123;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ZoomRecording>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ZoomReco__3214EC07CAF48162");

            entity.HasIndex(e => e.FileId, "UQ__ZoomReco__6F0F98BE30179AAA").IsUnique();

            entity.Property(e => e.DownloadedAt).HasColumnType("datetime");
            entity.Property(e => e.FileId).HasMaxLength(100);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.MeetingUuid).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
