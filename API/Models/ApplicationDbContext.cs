using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace API.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Livro> Livros { get; set; }

    public virtual DbSet<AccessLog> AccessLogs { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Utilizador> Utilizadors { get; set; }

    public virtual DbSet<Genero> Generos { get; set; }

    public virtual DbSet<Autor> Autores { get; set; }

    public virtual DbSet<Editora> Editoras { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Livro>(entity =>
        {
            entity.HasKey(e => e.IdLivro).HasName("PK__Livro__C252147DA63CC2B9");

            entity.ToTable("Livro");

            entity.Property(e => e.IdLivro).HasColumnName("id_livro");
            entity.Property(e => e.AnoPublicacao).HasColumnName("ano_publicacao");
            entity.Property(e => e.ISBN)
                .HasMaxLength(50)
                .HasColumnName("isbn");
            entity.Property(e => e.Idioma)
                .HasMaxLength(50)
                .HasColumnName("idioma");
            entity.Property(e => e.Sinopse)
                .HasMaxLength(2000)
                .HasColumnName("sinopse");
            entity.Property(e => e.CapaUrl)
                .HasMaxLength(500)
                .HasColumnName("capa_url");
            entity.Property(e => e.Estado)
                .HasMaxLength(30)
                .HasDefaultValue("ativo")
                .HasColumnName("estado");
            entity.Property(e => e.Titulo)
                .HasMaxLength(100)
                .HasColumnName("titulo");
            entity.Property(e => e.IdEditora).HasColumnName("id_editora");
            entity.Property(e => e.Clicks).HasColumnName("clicks").HasDefaultValue(0);

            entity.HasOne(d => d.EditoraNavigation).WithMany(p => p.Livros)
                .HasForeignKey(d => d.IdEditora)
                .HasConstraintName("FK_Livro_Editora");

            entity.HasMany(d => d.Generos).WithMany(p => p.Livros)
                .UsingEntity<Dictionary<string, object>>(
                    "LivroGenero",
                    l => l.HasOne<Genero>().WithMany()
                        .HasForeignKey("IdGenero")
                        .HasConstraintName("FK_LivroGenero_Genero"),
                    g => g.HasOne<Livro>().WithMany()
                        .HasForeignKey("IdLivro")
                        .HasConstraintName("FK_LivroGenero_Livro"),
                    j =>
                    {
                        j.HasKey("IdLivro", "IdGenero");
                        j.ToTable("LivroGenero");
                        j.IndexerProperty<int>("IdLivro").HasColumnName("id_livro");
                        j.IndexerProperty<int>("IdGenero").HasColumnName("id_genero");
                    });

            entity.HasMany(d => d.Autores).WithMany(p => p.Livros)
                .UsingEntity<Dictionary<string, object>>(
                    "LivroAutor",
                    l => l.HasOne<Autor>().WithMany()
                        .HasForeignKey("IdAutor")
                        .HasConstraintName("FK_LivroAutor_Autor"),
                    a => a.HasOne<Livro>().WithMany()
                        .HasForeignKey("IdLivro")
                        .HasConstraintName("FK_LivroAutor_Livro"),
                    j =>
                    {
                        j.HasKey("IdLivro", "IdAutor");
                        j.ToTable("LivroAutor");
                        j.IndexerProperty<int>("IdLivro").HasColumnName("id_livro");
                        j.IndexerProperty<int>("IdAutor").HasColumnName("id_autor");
                    });
        });

        modelBuilder.Entity<AccessLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_AccessLog");
            entity.ToTable("AccessLog");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Event).HasMaxLength(50).HasColumnName("event");
        });

        modelBuilder.Entity<Genero>(entity =>
        {
            entity.HasKey(e => e.IdGenero).HasName("PK__Genero__C252147DA63CC2B9");

            entity.ToTable("Genero");

            entity.Property(e => e.IdGenero).HasColumnName("id_genero");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Autor>(entity =>
        {
            entity.HasKey(e => e.IdAutor).HasName("PK__Autor__C252147DA63CC2B9");

            entity.ToTable("Autor");

            entity.Property(e => e.IdAutor).HasColumnName("id_autor");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Editora>(entity =>
        {
            entity.HasKey(e => e.IdEditora).HasName("PK__Editora__C252147DA63CC2B9");

            entity.ToTable("Editora");

            entity.Property(e => e.IdEditora).HasColumnName("id_editora");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.IdReserva).HasName("PK__Reserva__423CBE5D3753DD0B");

            entity.ToTable("Reserva");

            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.DataFim).HasColumnName("data_fim");
            entity.Property(e => e.DataInicio).HasColumnName("data_inicio");
            entity.Property(e => e.IdUtilizador).HasColumnName("id_utilizador");

            entity.HasOne(d => d.IdUtilizadorNavigation).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.IdUtilizador)
                .HasConstraintName("FK_Reserva_Utilizador");

            entity.HasMany(d => d.IdLivros).WithMany(p => p.IdReservas)
                .UsingEntity<Dictionary<string, object>>(
                    "ReservaLivro",
                    r => r.HasOne<Livro>().WithMany()
                        .HasForeignKey("IdLivro")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ReservaLivro_Livro"),
                    l => l.HasOne<Reserva>().WithMany()
                        .HasForeignKey("IdReserva")
                        .HasConstraintName("FK_ReservaLivro_Reserva"),
                    j =>
                    {
                        j.HasKey("IdReserva", "IdLivro");
                        j.ToTable("ReservaLivro");
                        j.IndexerProperty<int>("IdReserva").HasColumnName("id_reserva");
                        j.IndexerProperty<int>("IdLivro").HasColumnName("id_livro");
                    });
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.IdReview).HasName("PK__Review__2F79F8C7613DA0E9");

            entity.ToTable("Review");

            entity.HasIndex(e => new { e.IdUtilizador, e.IdLivro }, "UQ_Review_User_Livro").IsUnique();

            entity.Property(e => e.IdReview).HasColumnName("id_review");
            entity.Property(e => e.DataAprovacao).HasColumnName("data_aprovacao");
            entity.Property(e => e.DataSubmissao)
                .HasDefaultValueSql("(CONVERT([date],getdate()))")
                .HasColumnName("data_submissao");
            entity.Property(e => e.IdLivro).HasColumnName("id_livro");
            entity.Property(e => e.IdUtilizador).HasColumnName("id_utilizador");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("pendente")
                .HasColumnName("status");
            entity.Property(e => e.TextoReview)
                .HasMaxLength(255)
                .HasColumnName("texto_review");

            entity.HasOne(d => d.IdLivroNavigation).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.IdLivro)
                .HasConstraintName("FK_Review_Livro");

            entity.HasOne(d => d.IdUtilizadorNavigation).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.IdUtilizador)
                .HasConstraintName("FK_Review_Utilizador");
        });

        modelBuilder.Entity<Utilizador>(entity =>
        {
            entity.HasKey(e => e.IdUtilizador).HasName("PK__Utilizad__71C536835F89D131");

            entity.ToTable("Utilizador");

            entity.HasIndex(e => e.Email, "UQ__Utilizad__AB6E616464D5E079").IsUnique();

            entity.Property(e => e.IdUtilizador).HasColumnName("id_utilizador");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Nome)
                .HasMaxLength(100)
                .HasColumnName("nome");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.TipoUtilizador)
                .HasMaxLength(30)
                .HasColumnName("tipo_utilizador");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
