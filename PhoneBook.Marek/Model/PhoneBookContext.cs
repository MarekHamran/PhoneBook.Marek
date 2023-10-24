using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace PhoneBook.Marek.Model
{
    /// <summary>
    /// DB context class for phonebook
    /// </summary>
    public class PhoneBookContext : DbContext
    {
        /// <summary>
        /// DbSet for companies
        /// </summary>
        public DbSet<Company> Companies { get; set; }

        /// <summary>
        /// DbSet for persons
        /// </summary>
        public DbSet<Person> Persons { get; set; }

        public PhoneBookContext(DbContextOptions<PhoneBookContext> options)
            : base(options)
        {
        }

        public PhoneBookContext()
            : base()
        {
        }

        /// <summary>
        /// Overriden, setting up relation between <see cref="Person"/> and <see cref="Company0"/>
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Company>().HasMany(e => e.Persons).WithOne(e => e.Company).HasForeignKey(e => e.CompanyId);
            modelBuilder.Entity<Person>().HasOne(e => e.Company).WithMany(e => e.Persons).HasForeignKey(e => e.CompanyId);
        }
    }
}
