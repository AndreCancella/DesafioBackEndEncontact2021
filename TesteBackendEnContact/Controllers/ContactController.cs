using ChoETL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TesteBackendEnContact.Core.Domain.ContactBook.Contact;
using TesteBackendEnContact.Core.Interface.ContactBook.Contact;
using TesteBackendEnContact.Repository.Interface;
using System.Linq;
using TesteBackendEnContact.Repository;
using Microsoft.Data.Sqlite;
using TesteBackendEnContact.Database;

namespace TesteBackendEnContact.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly ILogger<ContactController> _logger;

        public ContactController(ILogger<ContactController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IContact> Post(Contact contact, [FromServices] IContactRespository contactRespository)
        {
            return await contactRespository.SaveAsync(contact);
        }
        [HttpPost]
        [Route("{caminhoCsv}")]
        public async Task<List<IContact>> Post(string caminhoCsv, [FromServices] IContactRespository contactRespository)
        {
            return await contactRespository.SaveAsyncCSV(caminhoCsv);
        }

        [HttpDelete]
        public async Task Delete(int id, [FromServices] IContactRespository contactRepository)
        {
            await contactRepository.DeleteAsync(id);
        }
        [HttpGet]
        public async Task<IEnumerable<IContact>> Get([FromServices] IContactRespository contactRespository)
        {
            return await contactRespository.GetAllAsync();
        }
        [HttpGet]
        [Route("pagina/{pag}")]
        public async Task<IEnumerable<IContact>> Get(int pag, [FromServices] IContactRespository contactRepository)
        {
            return await contactRepository.GetAllPag(pag);
        }
        [HttpGet("{id}")]
        public async Task<IContact> GetOne(int id, [FromServices] IContactRespository contactRepository)
        {
            return await contactRepository.GetAsync(id);
        }
        [HttpGet]
        [Route("pesquisa/{value}")]
        public async Task<IEnumerable<IContact>> GetPesquisa(string value, int pag, [FromServices] IContactRespository contactRepository)
        {
            return await contactRepository.Get(value, pag);
        }
        [HttpGet]
        [Route("company/{empresa}")]
        public async Task<IEnumerable<IContact>> GetEmrepsa(int empresa, int agenda, [FromServices] IContactRespository contactRepository)
        {
            return await contactRepository.GetAgendaEmpresa(empresa, agenda);
        }
    }
}
