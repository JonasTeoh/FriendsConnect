using Microsoft.EntityFrameworkCore;
using projectv2.Models;
using System;

namespace projectv2.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>
        options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
		public DbSet<Friend> Friends { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventHasFriend> EventHasFriends { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Kawan> Kawans { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Call the base method

            modelBuilder.Entity<Event>()
                    .HasOne(e => e.Organizer)
                    .WithMany() // One-to-many relationship
                    .HasForeignKey(e => e.OrganizerId) // Explicitly specify foreign key
                    .OnDelete(DeleteBehavior.Cascade);

            // Configure the FriendRequest relationship
            modelBuilder.Entity<FriendRequest>()
                .HasOne(f => f.Sender)
                .WithMany()
                .HasForeignKey(f => f.SenderId)
                .OnDelete(DeleteBehavior.Cascade); // Prevent deletion of sender when a request is deleted

            modelBuilder.Entity<FriendRequest>()
                .HasOne(f => f.Receiver)
                .WithMany()
                .HasForeignKey(f => f.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion of receiver when a request is deleted

            // Define the composite key for the Friend model
            modelBuilder.Entity<Kawan>()
                .HasKey(f => f.Id);

            // Define relationships between User and Friend
            modelBuilder.Entity<Kawan>()
                .HasOne(f => f.User)
                .WithMany() // A user can have many friendships
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Prevent deleting user if friendship exists

            modelBuilder.Entity<Kawan>()
                .HasOne(f => f.Friend)
                .WithMany() // A user can be a friend to many users
                .HasForeignKey(f => f.FriendId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting friend if friendship exists
        }
    }
}