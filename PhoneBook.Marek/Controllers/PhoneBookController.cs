using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using PhoneBook.Marek.Model;
using System;

namespace PhoneBook.Marek.Controllers
{
    /// <summary>
    /// API controller for phone book
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class PhoneBookController : ControllerBase
    {

        private readonly ILogger<PhoneBookController> _logger;
        private readonly PhoneBookContext _context;

        /// <summary>
        /// Constructor stores logger (not used yet) and <see cref="PhoneBookContext"/> in private fields
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="context"></param>
        public PhoneBookController(ILogger<PhoneBookController> logger, PhoneBookContext context)
        {
            _logger = logger;
            _context = context;
            //_context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        /// <summary>
        /// Adds company with unique <paramref name="Name"/>
        /// </summary>
        /// <param name="Name"></param>
        /// <returns>Created <see cref="Company"/></returns>
        [HttpGet("Company/Add")]
        public Company AddCompany(string Name)
        {
            Company company = new Company() { CompanyName = Name, RegistrationDate = DateTime.UtcNow };
            _context.Companies.Add(company);
            _context.SaveChanges();
            return company;
        }

        /// <summary>
        /// Gets all companies
        /// </summary>
        /// <returns>All companies</returns>
        [HttpGet("Company/GetAll")]
        public List<Company> GetAllCompanies()
        {
            return _context.Companies.Select(t => new Company { Id = t.Id, CompanyName = t.CompanyName, PersonCount = t.Persons.Count }).ToList();
        }

        /// <summary>
        /// Adds persons with passed parameters to <paramref name="CompanyName"/>
        /// </summary>
        /// <param name="FullName">Full name</param>
        /// <param name="PhoneNumber">Phone number</param>
        /// <param name="Address">Address</param>
        /// <param name="CompanyName">Company name (required)</param>
        /// <returns>Created person</returns>
        [HttpGet("Person/Add")]
        public ActionResult<Person> AddPerson(string FullName, string PhoneNumber, string Address, string CompanyName)
        {
            Company? company = _context.Companies.Where(t => t.CompanyName == CompanyName).FirstOrDefault();
            if (company == null) 
            {
                return new NotFoundObjectResult($"Company with CompanyName {CompanyName} doesn't exists.");
            }

            Person person = new Person { FullName = FullName, PhoneNumber = PhoneNumber, Address = Address, Company = company };
            _context.Persons.Add(person);

            _context.SaveChanges();
            return person;
        }

        /// <summary>
        /// Returns persons with any attribute containing searchTerm
        /// or all persons if searchTerm is not specified
        /// </summary>
        /// <param name="searchTerm">optional search term</param>
        /// <returns>Search result or all persons</returns>
        [HttpGet("Person/GetAll")]
        public List<Person> GetAllPersons(string? searchTerm = null)
        {
            IQueryable<Person> query = _context.Persons.Include(t => t.Company);
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(t =>
                    (!string.IsNullOrEmpty(t.FullName) && t.FullName.Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(t.Address) && t.Address.Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(t.PhoneNumber) && t.PhoneNumber.Contains(searchTerm)) ||
                    (!string.IsNullOrEmpty(t.Company.CompanyName) && t.Company.CompanyName.Contains(searchTerm)));
            }
            return query.ToList();
        }

        /// <summary>
        /// Randomly picks up person
        /// </summary>
        /// <returns>Random person or NotFound result if there is no person</returns>
        [HttpGet("Person/WildCard")]
        public ActionResult<Person> GetRandomPerson()
        {
            int count = _context.Persons.Count();
            if (count == 0)
            {
                return new NotFoundResult();
            }
            int random = Random.Shared.Next(0, count);

            long personId = _context.Persons.Select(t => t.Id).OrderBy(t => t).Take(random + 1).Last();
            Person person = _context.Persons.First(t => t.Id == personId);
            return person;
        }

        /// <summary>
        /// Update person attributes specified in <paramref name="person"/>
        /// </summary>
        /// <param name="person"><see cref="Person"/> with attributes to be updated</param>
        /// <returns>Updated person or NotFound result</returns>
        [HttpPost("Person/Update")]
        public ActionResult<Person> UpdatePerson( [FromBody] Person person)
        {
            Person? updated = _context.Persons.FirstOrDefault(t => t.Id == person.Id);
            if (updated != null)
            {
                updated.FullName = person.FullName;
                updated.Address = person.Address;
                updated.PhoneNumber = person.PhoneNumber;
                updated.Company = person.Company;
                
                _context.SaveChanges(); 
            }
            else
            {
                return new NotFoundObjectResult($"Person with Id {person.Id} doesn't exists.");
            }
            return updated;
        }

        /// <summary>
        /// Deletes person
        /// </summary>
        /// <param name="id">Id of person to by deleted</param>
        /// <returns>OK if person was deleted, or NotFound result</returns>
        [HttpGet("Person/Delete")]
        public ActionResult DeletePerson(long  id) 
        {
            Person? toDelete = _context.Persons.FirstOrDefault(t => t.Id == id);
            if (toDelete != null)
            {
                _context.Persons.Remove(toDelete);
                _context.SaveChanges();
                return Ok();
            }
            else
            {
                return new NotFoundObjectResult($"Person with Id {id} doesn't exists.");
            }
        }
    }
}