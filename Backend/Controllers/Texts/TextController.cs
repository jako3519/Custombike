using Backend.Data;
using Backend.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Route("api/texts")]
public class TextController : ControllerBase
{
    private readonly DataContext _context;

    public TextController(DataContext context)
    {
        _context = context;
    }

    // GET /api/texts  -> alle tekster med id, værdi og beskrivelse
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var texts = await _context.SiteTexts
            .AsNoTracking()
            .OrderBy(t => t.Id)
            .Select(t => new { t.Id, t.Value, t.Description })
            .ToListAsync();

        return Ok(texts);
    }

    // POST /api/texts  { "id": "about.title", "value": "Ny tekst" }
    // Id'et bestemmer hvilken tekst der ændres. Kræver at man er logget ind som admin.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateTextDto dto)
    {
        var text = await _context.SiteTexts.FindAsync(dto.Id);
        if (text == null)
        {
            return NotFound($"Der findes ingen tekst med id '{dto.Id}'. Se GET /api/texts for gyldige id'er.");
        }

        text.Value = dto.Value;
        await _context.SaveChangesAsync();

        return Ok(new { text.Id, text.Value, text.Description });
    }
}
