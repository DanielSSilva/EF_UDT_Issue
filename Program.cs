using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace wrong_type
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new SampleContext())
            {
				Console.WriteLine("Inserting a new Profile");
				db.Add(new Profile { Name = "SomeProfile" });
				db.SaveChanges();
				Profile addedProfile = db.Profiles.OrderBy(p => p.Id).First();
				Console.WriteLine("Inserting a new User");
				db.Add(new User { Name = "Albert", Age = 99 });
				db.SaveChanges();

				User addedUser = db.Users.OrderByDescending(u => u.Id).First();
				
				Console.WriteLine("Inserting a new UserProfile");
				db.Add(new UserProfile { ProfileId = addedProfile.Id, UserId = addedUser.Id });
				db.SaveChanges();
	
				List<long> usersIds = db.Set<User>().Select(user => user.Id).ToList();
				
				IEnumerable<UserProfile> userProfiles = db.Set<UserProfile>().Where(userProfile => usersIds.Contains(userProfile.UserId)).ToList();

			}
        }
    }

	 public class SampleContext : DbContext
    {
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<User> Users { get; set; }
		public DbSet<UserProfile> UserProfiles { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder options) 
		=> options.UseSqlServer("Server=.;Database=WrongTypeSample;User Id=SomeUser;Password=SomePassword;");

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);	           
		}
	}

	public class Profile
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public ICollection<UserProfile> UserProfiles { get; set; }
	}

	 public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long Age { get; set; }
		public ICollection<UserProfile> UserProfiles { get; set; }
    }

    public class UserProfile
    {
        public long ProfileId { get; set; }
        public long UserId { get; set; }
		public User User { get; set; }
		public Profile Profile { get; set; }
    }

	public class UserProfileConfig : IEntityTypeConfiguration<UserProfile>
	{
		public void Configure(EntityTypeBuilder<UserProfile> builder)
		{
			builder.HasKey(userProfile => new {userProfile.ProfileId, userProfile.UserId});
			builder.Property(p => p.UserId).HasColumnType("fid");
			builder.Property(p => p.ProfileId).HasColumnType("fid");

			builder.HasOne(up => up.Profile)
					.WithMany(p => p.UserProfiles)
					.HasForeignKey(up => up.ProfileId);
			
			builder.HasOne(up => up.User)
					.WithMany(u => u.UserProfiles)
					.HasForeignKey(up => up.UserId);
		
		}
	}
}


