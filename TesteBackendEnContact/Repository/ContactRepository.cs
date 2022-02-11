using ChoETL;
using Dapper;
using Dapper.Contrib.Extensions;
using GroupDocs.Conversion;
using GroupDocs.Conversion.Options.Convert;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook.Contact;
using TesteBackendEnContact.Core.Interface.ContactBook;
using TesteBackendEnContact.Core.Interface.ContactBook.Company;
using TesteBackendEnContact.Core.Interface.ContactBook.Contact;
using TesteBackendEnContact.Database;
using TesteBackendEnContact.Repository.Interface;
using Aspose.Cells;
using Aspose.Cells.Utility;

namespace TesteBackendEnContact.Repository
{
    public class ContactRepository : IContactRespository
    {
        private readonly DatabaseConfig databaseConfig;
        public ContactRepository(DatabaseConfig databaseConfig)
        {
            this.databaseConfig = databaseConfig;
        }

        public async Task<IContact> SaveAsync(IContact contact)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            ContactDao dao;

            if (contact.Id == 0)
            {
                dao = new ContactDao(contact);
            }
            else
            {
                dao = (ContactDao)await GetAsync(contact.Id);
            }


            dao.Id = await connection.InsertAsync(dao);

            return dao.Export();
        }
        public async Task<List<IContact>> SaveAsyncCSV(string camiinho)
        { 
            var reader = new StreamReader(File.OpenRead(camiinho));
            var listCsvContact = new List<Contact>();

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');

                var tempContact = new Contact(
                    int.Parse(values[0]),
                    int.Parse(values[1]),
                    int.Parse(values[2]),
                    values[3],
                    values[4],
                    values[5],
                    values[6]);

                listCsvContact.Add(tempContact);
            }

            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var daoReturn = new List<IContact>();

            foreach (var contactCsv in listCsvContact)
            {

                try
                {
                    var dao = new ContactDao(contactCsv);

                    dao.Id = await connection.InsertAsync(dao);

                    daoReturn.Add(dao.Export());
                }
                catch (System.Exception)
                {
                    //não atrapalha a importação.
                }
            }

            return daoReturn;
        }

        public async Task DeleteAsync(int id)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Contact WHERE Id = @id;";
            command.Parameters.AddWithValue("id", id);
            command.ExecuteNonQuery();
            transaction.Commit();
        }


        public async Task<IEnumerable<IContact>> GetAllAsync()
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);

            var querry = "SELECT *  FROM  Contact";
            return await connection.QueryAsync<ContactDao>(querry);

            
        }
        public async Task<IEnumerable<IContact>> GetAllPag(int pag)
        {
            using var connection = new SqliteConnection(databaseConfig.ConnectionString);
            int limit = 5;
            var offset = 0;


            if(pag > 1)
            {
                offset = (limit * (pag - 1));
            }

            var querry = "SELECT *  FROM  Contact LIMIT " + limit+ " OFFSET " + offset;
            return await connection.QueryAsync<ContactDao>(querry);

        }
        public async Task<IContact> GetAsync(int id)
        {
            var list = await GetAllAsync();

            return list.ToList().FirstOrDefault(item => item.Id == id);
        }
        public async Task<IEnumerable<IContact>> Get(string value, int pag)
        {
            var list = await GetAllPag(pag);
            int id;
            IEnumerable<IContact> lista;

            if (Int32.TryParse(value, out id))
            {
                return list.Where(item => item.Id == id);
            }
            lista = list.ToList().Where(item => item.Name.Contains(value));

            if(lista.Count() > 0)
            {
                return lista;
            }

            Regex regex = new Regex("^([1-9]{2}) (?:[2-8]|9[1-9])[0-9]{3}-[0-9]{4}$");
            if (regex.IsMatch(value))
            {
                return list.ToList().Where(item => item.Phone == value);
            }
            Regex rgEmail = new Regex("^[a-z0-9.]+@[a-z0-9]+.[a-z]+.([a-z]+)?$");
            if (rgEmail.IsMatch(value))
            {
                return list.ToList().Where(item => item.Email == value);
            }

            lista = list.ToList().Where(item => item.Address.Contains(value));
            if (lista.Count() > 0)
            {
                return lista;
            }

            CompanyRepository company = new CompanyRepository(databaseConfig);
            var companyList = await company.GetAllAsync();
            var companyResult = companyList.FirstOrDefault(item => item.Name.Contains(value));
            
            if(companyResult != null)
            {
                lista = list.Where(item => item.CompanyId == companyResult.Id);
                if (lista.Count() > 0)
                {
                    return lista;
                }
            }

            ContactBookRepository contactBook = new ContactBookRepository(databaseConfig);
            var contactBookList = await contactBook.GetAllAsync();
            var contactBookResult = contactBookList.FirstOrDefault(item => item.Name.Contains(value));
            
            if(contactBookResult != null)
            {
                lista = list.Where(item => item.ContactBookId == contactBookResult.Id);
                if (lista.Count() > 0)
                {
                    return lista;
                }
            }

            return null;
        }
        public async Task<IEnumerable<IContact>> GetAgendaEmpresa(int empresa, int agenda)
        {
            var list = await GetAllAsync();

            return list.ToList().Where(item => item.CompanyId == empresa && item.ContactBookId == agenda);
        }
    }

    [Table("Contact")]
    public class ContactDao : IContact
    {
        [Key]
        public int Id { get; set; }

        public int ContactBookId { get; set; }

        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }

        public string Email { get; set; }

        public string Address { get; set; }

        public ContactDao()
        {
        }

        public ContactDao(IContact contact)
        {
            Id = contact.Id;
            ContactBookId = contact.ContactBookId;
            CompanyId = contact.CompanyId;
            Name = contact.Name;
            Phone = contact.Phone;
            Email = contact.Email;
            Address = contact.Address;
        }

        public IContact Export() => new Contact(Id, ContactBookId, CompanyId, Name, Phone, Email, Address);
    }
}
