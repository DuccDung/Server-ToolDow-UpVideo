using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Server_ToolDow_UpVideo.Models;

namespace Server_ToolDow_UpVideo.Contexts;

public partial class InformationMeetingContext : DbContext
{
    public InformationMeetingContext(DbContextOptions<InformationMeetingContext> options)
        : base(options)
    {
    }

    public virtual DbSet<GoogleToken> GoogleToken { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GoogleToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GoogleTo__3214EC072880BADA");

            entity.Property(e => e.IssuedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Scope).HasMaxLength(500);
            entity.Property(e => e.TokenType).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
