using API.EF.Api.Context;
using API.EF.Api.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API.EF.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContatoController : ControllerBase
{
    private readonly AgendaContext _context;
    public ContatoController(AgendaContext context) => _context = context;

    [HttpPost]
    public IActionResult Create(Contato contato)
    {
        _context.Add(contato);
        _context.SaveChanges();
        return Ok(contato);
    }

    [HttpGet("{id}")]
    public IActionResult ObterPorId(int id)
    {
        var contato = _context.Contatos.Find(id);

        if (contato is null)
            return NotFound();

        return Ok(contato);
    }

    [HttpGet]
    public IActionResult ObterTodos()
    {
        var contatos = _context.Contatos.ToList();

        return Ok(contatos);
    }

    [HttpGet("ObterPorNome")]
    public IActionResult ObterPorNome(string nome)
    {
        var contatos = _context.Contatos.Where(c => c.Nome.Contains(nome));
        return Ok(contatos);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, Contato contato)
    {
        var contatoBanco = _context.Contatos.Find(id);

        if (contatoBanco is null)
            return NotFound();

        contatoBanco.Nome = contato.Nome;
        contatoBanco.Telefone = contato.Telefone;
        contatoBanco.Ativo = contato.Ativo;

        _context.Contatos.Update(contatoBanco);
        _context.SaveChanges();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var contatoBanco = _context.Contatos.Find(id);

        if (contatoBanco is null)
            return NotFound();

        _context.Remove(contatoBanco);
        _context.SaveChanges();
        return NoContent();
    }

}
