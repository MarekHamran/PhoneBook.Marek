using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhoneBook.Marek.Model
{
    /// <summary>
    /// Person class represention person record in phone book
    /// </summary>
    [Table(nameof(Person))]
    public class Person
    {
        /// <summary>
        /// Id - assigned by ms sql
        /// </summary>
        [Key()]
        public long Id { get; set; }

        /// <summary>
        /// Full name of person
        /// </summary>
        [Required]
        [StringLength(160)]
        public string? FullName { get; set; }

        /// <summary>
        /// Phone number as string with required format
        /// </summary>
        [Phone]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Address
        /// </summary>
        [StringLength(200)]
        public string? Address { get; set; }

        /// <summary>
        /// <see cref="Company.Id"/> of person
        /// </summary>
        public long CompanyId { get; set; }

        /// <summary>
        /// <see cref="Company"/> of person
        /// </summary>
        public Company Company { get; set; } = null!;
    }
}
