using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DynamicSearch.Test
{
    public class ChinookContext : DbContext
    {
        public DbSet<Artist> Artist { get; set; }
        public DbSet<Album> Album { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=Chinook.db");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new ArtistMap());
            modelBuilder.ApplyConfiguration(new AlbumMap());
        }
    }

    public class Artist
    {
        public int ArtistId { get; set; }
        public string Name { get; set; }
    }

    public class Album
    {
        public int AlbumId { get; set; }
        public string Title { get; set; }
        public int ArtistId { get; set; }
    }

    public class ArtistMap : IEntityTypeConfiguration<Artist>
    {
        public void Configure(EntityTypeBuilder<Artist> builder)
        {
            builder.ToTable("Artist");
            builder.HasKey(x => x.ArtistId);
            builder.Property(x => x.ArtistId).HasColumnName("ArtistId");
            builder.Property(x => x.Name).HasColumnName("Name");
        }
    }

    public class AlbumMap : IEntityTypeConfiguration<Album>
    {
        public void Configure(EntityTypeBuilder<Album> builder)
        {
            builder.ToTable("Album");
            builder.HasKey(x => x.AlbumId);
            builder.Property(x => x.AlbumId).HasColumnName("AlbumId");
            builder.Property(x => x.Title).HasColumnName("Title");
            builder.Property(x => x.ArtistId).HasColumnName("ArtistId");
        }
    }
}