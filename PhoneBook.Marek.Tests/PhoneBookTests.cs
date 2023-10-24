using Azure;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PhoneBook.Marek.Controllers;
using PhoneBook.Marek.Model;
using System;
using System.Net.Http.Json;

namespace PhoneBook.Marek.Tests
{
    [TestClass]
    public class PhoneBookTests
    {
        private static WebApplicationFactory<Program> _factory;
        private static HttpClient _httpClient;
        private static PhoneBookContext _context;
        private static IServiceScope _scope;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
                    {
                        builder.UseSetting("https_port", "5001");
                    });
            _httpClient = _factory.CreateClient();

            var serviceScopeFactory = _factory.Services.GetService<IServiceScopeFactory>();
            _scope = serviceScopeFactory.CreateScope();
            _context = (PhoneBookContext)_scope.ServiceProvider.GetService(typeof(PhoneBookContext));
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _httpClient.Dispose();
            _factory.Dispose();
            _context.Dispose();
            _scope.Dispose();
        }

        [TestMethod]
        [DataRow("Writers")]
        [DataRow("Painters")]
        public void Company_Add(string name)
        {
            // test
            try
            {
                var response = _httpClient.GetAsync($"/PhoneBook/Company/Add?Name={name}").Result;

                var result = response.Content.ReadAsStringAsync().Result;
                var company = JsonConvert.DeserializeObject<Company>(result);

                Assert.IsNotNull(company);
                Assert.IsTrue(company.Id > 0);
            }
            finally
            // cleanup
            {
                Company? toDelete = _context.Companies.Where(t => t.CompanyName == name).FirstOrDefault();
                if (toDelete != null)
                {
                    _context.Remove(toDelete);
                    _context.SaveChanges();
                }
            }
        }

        [TestMethod]
        public void Company_GetAll()
        {
            // setup
            Company writers = new Company { CompanyName = "Writers" };
            _context.Companies.Add(writers);
            Person writer1 = new Person { FullName = "William Shakespeare", PhoneNumber = "1234567890" };
            Person writer2 = new Person { FullName = "Dante Alighieri", PhoneNumber = "1234567890" };
            writers.Persons.Add(writer1);
            writers.Persons.Add(writer2);
            _context.SaveChanges();

            // test
            try
            {
                var response = _httpClient.GetAsync($"/PhoneBook/Company/GetAll").Result;

                var result = response.Content.ReadAsStringAsync().Result;
                var companies = JsonConvert.DeserializeObject<List<Company>>(result);
                Assert.IsNotNull(companies);
                Assert.IsTrue(companies.Single(t => t.CompanyName == "Writers").PersonCount == 2);
            }
            finally
            // cleanup
            {
                _context.Persons.Remove(writer1);
                _context.Persons.Remove(writer2);
                _context.Companies.Remove(writers);
                _context.SaveChanges();
            }
        }

        [TestMethod]
        [DataRow("William Shakespeare", "1234567890", "Writers", "Street 1")]
        [DataRow("Dante Alighieri", "1234567890", "Writers", "Street 1")]
        public void Person_Add(string name, string phoneNumber, string companyName, string address)
        {
            //setup
            Company writers = new Company { CompanyName = companyName };
            _context.Companies.Add(writers);
            _context.SaveChanges();

            long idToClean = 0;
            // test
            try
            {
                var response = _httpClient.GetAsync($"/PhoneBook/Person/Add?FullName={name}&PhoneNumber={phoneNumber}&CompanyName={companyName}&Address={address}").Result;

                var result = response.Content.ReadAsStringAsync().Result;
                var person = JsonConvert.DeserializeObject<Person>(result);

                Assert.IsNotNull(person);

                idToClean = person.Id;
                Assert.IsTrue(person.Id > 0);
            }
            finally
            // cleanup
            {
                if (idToClean > 0)
                {
                    Person? toDelete = _context.Persons.Where(t => t.Id == idToClean).FirstOrDefault();
                    if (toDelete != null)
                    {
                        _context.Remove(toDelete);
                    }
                }
                _context.Remove(writers);
                _context.SaveChanges(true);
            }
        }

        [TestMethod]
        [DataRow("William Shakespeare", "William", "1234567890", "Writers", "Street 1")]
        [DataRow("Dante Alighieri", "Dante", "1234567890", "Writers", "Street 1")]
        public void Person_AddEditRemove(string name, string newName, string phoneNumber, string companyName, string address)
        {
            // setup
            Company writers = new Company { CompanyName = companyName };
            _context.Companies.Add(writers);
            _context.SaveChanges();

            long idToClean = 0;
            // test
            try
            {
                var response = _httpClient.GetAsync($"/PhoneBook/Person/Add?FullName={name}&PhoneNumber={phoneNumber}&CompanyName={companyName}&Address={address}").Result;
                var result = response.Content.ReadAsStringAsync().Result;
                var person = JsonConvert.DeserializeObject<Person>(result);

                Assert.IsNotNull(person);

                idToClean = person.Id;
                Assert.IsTrue(person.Id > 0);

                person.FullName = newName;

                response = _httpClient.PostAsJsonAsync($"/PhoneBook/Person/Update", person).Result;
                result = response.Content.ReadAsStringAsync().Result;
                person = JsonConvert.DeserializeObject<Person>(result);

                Assert.IsNotNull(person);
                Assert.IsTrue(person.FullName == newName);

                response = _httpClient.GetAsync($"/PhoneBook/Person/Delete?id={person.Id}").Result;
                Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.OK);
            }
            finally
            // cleanup
            {
                if (idToClean > 0)
                {
                    Person? toDelete = _context.Persons.Where(t => t.Id == idToClean).FirstOrDefault();
                    if (toDelete != null)
                    {
                        _context.Remove(toDelete);
                    }
                }
                _context.Remove(writers);
                _context.SaveChanges(true);
            }
        }

        [TestMethod]
        public void Person_GetAll()
        {
            // setup
            Company writers = new Company { CompanyName = "Writers" };
            _context.Companies.Add(writers);
            Person writer1 = new Person { FullName = "William Shakespeare", PhoneNumber = "1234567890" };
            Person writer2 = new Person { FullName = "Dante Alighieri", PhoneNumber = "1234567890" };
            writers.Persons.Add(writer1);
            writers.Persons.Add(writer2);
            _context.SaveChanges();

            // test
            try
            {
                var response = _httpClient.GetAsync($"/PhoneBook/Person/GetAll").Result;
                var result = response.Content.ReadAsStringAsync().Result;
                var persons = JsonConvert.DeserializeObject<List<Person>>(result);

                Assert.IsNotNull(persons);
                Assert.IsTrue(persons.Any(t => t.FullName == writer1.FullName));
                Assert.IsTrue(persons.Any(t => t.FullName == writer2.FullName));
            }
            finally
            // cleanup
            {
                _context.Persons.Remove(writer1);
                _context.Persons.Remove(writer2);
                _context.Companies.Remove(writers);
                _context.SaveChanges();
            }
        }

        [TestMethod]
        public void Person_Search()
        {
            // setup
            Company writers = new Company { CompanyName = "Writers" };
            _context.Companies.Add(writers);
            Person writer1 = new Person { FullName = "William Shakespeare", PhoneNumber = "1234567890" };
            Person writer2 = new Person { FullName = "Dante Alighieri", PhoneNumber = "1234567890" };
            writers.Persons.Add(writer1);
            writers.Persons.Add(writer2);
            _context.SaveChanges();

            // test
            try
            {
                var response = _httpClient.GetAsync($"/PhoneBook/Person/GetAll?searchTerm=123456789").Result;
                var result = response.Content.ReadAsStringAsync().Result;
                var persons = JsonConvert.DeserializeObject<List<Person>>(result);

                Assert.IsNotNull(persons);
                Assert.IsTrue(persons.Any(t => t.FullName == writer1.FullName));
                Assert.IsTrue(persons.Any(t => t.FullName == writer2.FullName));

                response = _httpClient.GetAsync($"/PhoneBook/Person/GetAll?searchTerm=William").Result;
                result = response.Content.ReadAsStringAsync().Result;
                persons = JsonConvert.DeserializeObject<List<Person>>(result);

                Assert.IsNotNull(persons);
                Assert.IsTrue(persons.Any(t => t.FullName == writer1.FullName));
                Assert.IsTrue(!persons.Any(t => t.FullName == writer2.FullName));

                response = _httpClient.GetAsync($"/PhoneBook/Person/GetAll?searchTerm=5t34y895yt45980t3ut403").Result;
                result = response.Content.ReadAsStringAsync().Result;
                persons = JsonConvert.DeserializeObject<List<Person>>(result);

                Assert.IsNotNull(persons);
                Assert.IsTrue(persons.Count == 0);
            }
            finally
            // cleanup
            {
                _context.Persons.Remove(writer1);
                _context.Persons.Remove(writer2);
                _context.Companies.Remove(writers);
                _context.SaveChanges();
            }
        }

        [TestMethod]
        public void Person_WildCard()
        {
            // setup
            Company writers = new Company { CompanyName = "Writers" };
            _context.Companies.Add(writers);
            Person writer1 = new Person { FullName = "William Shakespeare", PhoneNumber = "1234567890" };
            Person writer2 = new Person { FullName = "Dante Alighieri", PhoneNumber = "1234567890" };
            writers.Persons.Add(writer1);
            writers.Persons.Add(writer2);
            _context.SaveChanges();

            try
            {
                var response = _httpClient.GetAsync($"/PhoneBook/Person/WildCard").Result;
                var result = response.Content.ReadAsStringAsync().Result;
                var person = JsonConvert.DeserializeObject<Person>(result);

                Assert.IsNotNull(person);
            }
            finally 
            {
                _context.Persons.Remove(writer1);
                _context.Persons.Remove(writer2);
                _context.Companies.Remove(writers);
                _context.SaveChanges();
            }
        }
    }
}