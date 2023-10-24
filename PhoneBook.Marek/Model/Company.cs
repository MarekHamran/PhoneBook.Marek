using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PhoneBook.Marek.Model
{
    /// <summary>
    /// Company class
    /// </summary>
    [Table(nameof(Company)), PrimaryKey(nameof(Id))]
    [Index(nameof(CompanyName), IsUnique = true)]
    public class Company
    {
        /// <summary>
        /// Id (set by ms sql)
        /// </summary>
        [Required]
        public long Id { get; set; }

        /// <summary>
        /// Unique company name
        /// </summary>
        [Required]
        [StringLength(120)]
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Registration date, set automatically during creation of company record
        /// </summary>
        public DateTime RegistrationDate {  get; set; }

        /// <summary>
        /// Person count, returned from db
        /// </summary>
        [NotMapped]
        public int PersonCount { get; set; }

        /// <summary>
        /// List of <see cref="Person"/>, not returned by API 
        /// </summary>
        [JsonIgnore]
        public List<Person> Persons { get; set; } = new List<Person>();

    }
}
