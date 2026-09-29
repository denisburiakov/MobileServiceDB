using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using MobileServiceSite.Models;

namespace MobileServiceSite.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<ClientsContact> ClientsContacts { get; set; }

    public virtual DbSet<Detail> Details { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql("Host=db;Port=5432;Database=my_database_name;Username=my_db_user;Password=my_db_password");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pkey");

            entity.ToTable("categories");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Categories)
                .HasMaxLength(50)
                .HasColumnName("categories");
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("clients_pkey");

            entity.ToTable("clients");

            entity.HasIndex(e => new { e.FirstName, e.LastName }, "idx_clients_first_name_last_name");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
        });

        modelBuilder.Entity<ClientsContact>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("clients_contact");

            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .HasColumnName("phone");
        });

        modelBuilder.Entity<Detail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("details_pkey");

            entity.ToTable("details");

            entity.HasIndex(e => e.DeviceId, "idx_details_device_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CostOfDetail).HasColumnName("cost_of_detail");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.ProducerDet)
                .HasMaxLength(50)
                .HasColumnName("producer_det");
            entity.Property(e => e.TypeOfDetail)
                .HasMaxLength(50)
                .HasColumnName("type_of_detail");

            entity.HasOne(d => d.Device).WithMany(p => p.Details)
                .HasForeignKey(d => d.DeviceId)
                .HasConstraintName("details_device_id_fkey");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("devices_pkey");

            entity.ToTable("devices");

            entity.HasIndex(e => e.SerialNumber, "idx_devices_serial_number");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.DefDescriotion)
                .HasMaxLength(50)
                .HasColumnName("def_descriotion");
            entity.Property(e => e.Model)
                .HasMaxLength(50)
                .HasColumnName("model");
            entity.Property(e => e.Producer)
                .HasMaxLength(50)
                .HasColumnName("producer");
            entity.Property(e => e.SerialNumber)
                .HasMaxLength(50)
                .HasColumnName("serial_number");
            entity.Property(e => e.TypeOfDevice)
                .HasMaxLength(50)
                .HasColumnName("type_of_device");

            entity.HasOne(d => d.Client).WithMany(p => p.Devices)
                .HasForeignKey(d => d.ClientId)
                .HasConstraintName("devices_client_id_fkey");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("services_pkey");

            entity.ToTable("services");

            entity.HasIndex(e => e.DeviceId, "idx_services_device_id");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CostOfService).HasColumnName("cost_of_service");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.TimeOfDoing)
                .HasMaxLength(50)
                .HasColumnName("time_of_doing");
            entity.Property(e => e.TypeOfService)
                .HasMaxLength(50)
                .HasColumnName("type_of_service");

            entity.HasOne(d => d.Category).WithMany(p => p.Services)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("services_category_id_fkey");

            entity.HasOne(d => d.Device).WithMany(p => p.Services)
                .HasForeignKey(d => d.DeviceId)
                .HasConstraintName("services_device_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
