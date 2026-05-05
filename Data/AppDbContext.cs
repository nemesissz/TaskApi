using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TaskApi.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<TaskAssignment> TaskAssignments => Set<TaskAssignment>();
        public DbSet<TaskComment> TaskComments => Set<TaskComment>();
        public DbSet<TaskFile> TaskFiles => Set<TaskFile>();
        public DbSet<Announcement> Announcements => Set<Announcement>();
        public DbSet<AnnouncementRead> AnnouncementReads => Set<AnnouncementRead>();
        public DbSet<ActivityLogItem> ActivityLogs => Set<ActivityLogItem>();
        public DbSet<Muessise> Muessiseler => Set<Muessise>();
        public DbSet<Bolme> Bolmeler => Set<Bolme>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<Note> Notes => Set<Note>();
        public DbSet<SubTask> SubTasks => Set<SubTask>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // NoAction: Muessise silinəndə AppUser.MuessiseId-ni silməzdən əvvəl
            // kodda null etmək lazımdır (MSSQL çoxlu cascade yoluna icazə vermir)
            builder.Entity<AppUser>()
                .HasOne(u => u.Muessise)
                .WithMany(m => m.Users)
                .HasForeignKey(u => u.MuessiseId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<AppUser>()
                .HasOne(u => u.Bolme)
                .WithMany(b => b.Users)
                .HasForeignKey(u => u.BolmeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Bolme>()
                .HasOne(b => b.Muessise)
                .WithMany(m => m.Bolmeler)
                .HasForeignKey(b => b.MuessiseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TaskItem>()
                .HasOne(t => t.Creator)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<TaskAssignment>()
                .HasOne(ta => ta.Task)
                .WithMany(t => t.Assignments)
                .HasForeignKey(ta => ta.TaskId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<TaskAssignment>()
                .HasOne(ta => ta.Assignee)
                .WithMany(u => u.Assignments)
                .HasForeignKey(ta => ta.AssigneeId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
