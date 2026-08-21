using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profynus.Domain.Audio.Entities;

namespace Profynus.Infrastructure.Persistence.Configurations;

public class SongConfiguration : IEntityTypeConfiguration<Song>
{
    public void Configure(EntityTypeBuilder<Song> b)
    {
        b.ToTable("songs");
        b.HasKey(x => x.Id);
        
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x=> x.SongName).HasColumnName("song_name");
        b.Property(x => x.ArtistName).HasColumnName("artist_name");
        b.Property(x => x.AlbumName).HasColumnName("album_name");
        b.Property(x=> x.Genre).HasColumnName("genre");
        b.Property(x => x.ReleaseYear).HasColumnName("release_year");
        b.Property(x => x.DurationSeconds).HasColumnName("duration_secs");
        b.Property(x => x.Language).HasColumnName("language");
        b.Property(x => x.SourceProvider).HasColumnName("source_provider");
        b.Property(x => x.SourceId).HasColumnName("source_id");
        b.Property(x => x.FileFormat).HasColumnName("file_format");
        b.Property(x=>x.FileSizeBytes).HasColumnName("file_size_bytes");
        b.Property(x => x.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
    }
}