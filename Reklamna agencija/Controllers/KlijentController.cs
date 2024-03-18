using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reklamna_agencija.Data;
using Reklamna_agencija.Models;

namespace Reklamna_agencija.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KlijentController : ControllerBase
    {
        private readonly DataContext _context;

        public KlijentController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Reklama>>> GetReklame()
        {
            return await _context.Reklame.ToListAsync();
        }

        [HttpGet("neodobrene-reklame")]
        public async Task<ActionResult<IEnumerable<Reklama>>> GetNeodobreneReklame()
        {
            var neodobreneReklame = await _context.Reklame
                .Where(r => r.Status == false)
                .ToListAsync();
    
            if (!neodobreneReklame.Any())
            {
                return NotFound();
            }

            // Eksplicitno učitavanje podataka o klijentu i reklamnom panou za svaku reklamu
            foreach (var reklama in neodobreneReklame)
            {
                await _context.Entry(reklama)
                    .Reference(r => r.Klijent)
                    .LoadAsync();
        
                await _context.Entry(reklama)
                    .Reference(r => r.ReklamniPano)
                    .LoadAsync();
            }

            return neodobreneReklame;
        }
        [HttpGet("odobrene-reklame")]
        public async Task<ActionResult<IEnumerable<Reklama>>> GetOdobreneReklame()
        {
            var neodobreneReklame = await _context.Reklame
                .Where(r => r.Status == true)
                .ToListAsync();

            if (!neodobreneReklame.Any())
            {
                return NotFound();
            }

            // Eksplicitno učitavanje podataka o klijentu i reklamnom panou za svaku reklamu
            foreach (var reklama in neodobreneReklame)
            {
                await _context.Entry(reklama)
                    .Reference(r => r.Klijent)
                    .LoadAsync();

                await _context.Entry(reklama)
                    .Reference(r => r.ReklamniPano)
                    .LoadAsync();
            }

            return neodobreneReklame;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Reklama>> GetReklama(int id)
        {
            var reklama = await _context.Reklame.FindAsync(id);

            if (reklama == null)
            {
                return NotFound();
            }

            return reklama;
        }

        [HttpGet("klijent-reklame/{id}")]
        public async Task<ActionResult<IEnumerable<Reklama>>> GetReklameByKlijentId(int id)
        {
            var reklame = await _context.Reklame.Where(r => r.KlijentId == id).ToListAsync();

            if (!reklame.Any())
            {
                return NotFound();
            }

            return reklame;
        }

        [HttpPost("dodaj-reklamu")]
        public async Task<ActionResult<Reklama>> PostReklama(Reklama reklama)
        {
            _context.Reklame.Add(reklama);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReklama), new { id = reklama.Id }, reklama);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutReklama(int id, Reklama reklama)
        {
            if (id != reklama.Id)
            {
                return BadRequest();
            }

            _context.Entry(reklama).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReklamaExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpPut("odobri-reklamu/{id}")]
        public async Task<IActionResult> OdobriReklamuIKreirajFakturu(int id)
        {
            var reklama = await _context.Reklame
                .Include(r => r.ReklamniPano)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reklama == null)
            {
                return NotFound();
            }

            try
            {
                reklama.Status = true;
                _context.Entry(reklama).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                // Izračunavanje broja dana aktivnosti reklame
                int brojDana = (int)(reklama.DoDatum - reklama.OdDatum).TotalDays;

                // Izračunavanje iznosa novčani na osnovu cene reklamnog panoa i broja dana
                decimal iznosNovcani = (decimal)(reklama.ReklamniPano.Cijena * brojDana);

                // Kreiranje nove fakture
                var novaFaktura = new Faktura
                {
                    AdminAgencijeId = reklama.ReklamniPano.AdminAgencijeId,
                    ReklamaId = reklama.Id,
                    Datum = DateTime.UtcNow,
                    IznosNovcani = iznosNovcani.ToString(), // Pretvaranje iznosa novčani u string
                    Status = true
                };

                _context.Fakture.Add(novaFaktura);
                await _context.SaveChangesAsync();

                return Ok("Reklama je uspešno odobrena, a faktura je kreirana.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Greška prilikom odobravanja reklame i kreiranja fakture: {ex.Message}");
            }
        }


        [HttpDelete("obrisi-reklamu/{id}")]
        public async Task<IActionResult> DeleteReklama(int id)
        {
            var reklama = await _context.Reklame.FindAsync(id);
            if (reklama == null)
            {
                return NotFound();
            }

            _context.Reklame.Remove(reklama);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReklamaExists(int id)
        {
            return _context.Reklame.Any(e => e.Id == id);
        }
    }
}
