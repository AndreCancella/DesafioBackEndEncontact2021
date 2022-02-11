using System.Collections.Generic;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Interface.ContactBook.Contact;

namespace TesteBackendEnContact.Repository.Interface
{
    public interface IContactRespository
    {
        Task<IContact> SaveAsync(IContact contact);
        Task DeleteAsync(int id);
        Task<IEnumerable<IContact>> GetAllAsync();
        Task<IContact> GetAsync(int id);
        Task<List<IContact>> SaveAsyncCSV(string caminho);
        Task<IEnumerable<IContact>> GetAllPag(int id);
        Task<IEnumerable<IContact>> Get(string value, int pag);
        Task<IEnumerable<IContact>> GetAgendaEmpresa(int empresa, int agenda);
    }
}
