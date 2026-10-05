using Microsoft.EntityFrameworkCore;
using TaskFlowApi.Entities;
using TaskStatus = TaskFlowApi.Entities.TaskStatus;

namespace TaskFlowApi.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Project>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasMany(x => x.Tasks).WithOne(x => x.Project).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            e.HasData(new Project { Id = 1, Name = "Демо-проект", Description = "Первый проект TaskFlow", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
        });
        model.Entity<TaskItem>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Priority).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => x.Status);
            e.HasOne(x => x.AssignedTo).WithMany(x => x.AssignedTasks).HasForeignKey(x => x.AssignedToId).OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Comments).WithOne(x => x.TaskItem).HasForeignKey(x => x.TaskItemId).OnDelete(DeleteBehavior.Cascade);
            e.HasData(new TaskItem { Id = 1, ProjectId = 1, Title = "Подготовить техническое задание", Status = TaskStatus.ToDo, Priority = Priority.Medium, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
        });
        model.Entity<AppUser>(e =>
        {
            e.Property(x => x.Username).HasMaxLength(80).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.PasswordHash).IsRequired();
            e.HasIndex(x => x.Username).IsUnique();
            e.HasData(new AppUser { Id = 1, Username = "demo", Email = "demo@example.local", PasswordHash = "demo-only-no-login" });
        });
        model.Entity<Comment>(e =>
        {
            e.Property(x => x.Content).HasMaxLength(500).IsRequired();
            e.HasOne(x => x.Author).WithMany(x => x.Comments).HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<IdempotencyRecord>(e =>
        {
            e.HasKey(x => x.Key);
            e.Property(x => x.Key).HasMaxLength(36);
            e.Property(x => x.RequestBodyHash).HasMaxLength(64).IsRequired();
            e.Property(x => x.ResponseBody).IsRequired();
        });
    }
}
