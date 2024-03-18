using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reklamna_agencija.Data;
using Reklamna_agencija.Models;
using System.Net;
using System.Net.Mail;

namespace Reklamna_agencija.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminAgencijeController : ControllerBase
    {
        private readonly DataContext _context;

        public AdminAgencijeController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReklamniPano>>> GetReklamniPanoi()
        {
            return await _context.ReklamniPanoi.ToListAsync();
        }

        [HttpGet("ReklamniPanoiNotInReklame")]
        public async Task<ActionResult<IEnumerable<ReklamniPano>>> GetReklamniPanoiNotInReklame()
        {
            var reklamniPanoiNotInReklame = await _context.ReklamniPanoi
                .Where(rp => !_context.Reklame.Any(r => r.ReklamniPanoId == rp.Id))
                .ToListAsync();

            return reklamniPanoiNotInReklame;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ReklamniPano>> GetReklamniPano(int id)
        {
            var reklamniPano = await _context.ReklamniPanoi.FindAsync(id);

            if (reklamniPano == null)
            {
                return NotFound();
            }

            return reklamniPano;
        }

        [HttpPost]
        public async Task<ActionResult<ReklamniPano>> PostReklamniPano(ReklamniPano reklamniPano)
        {
            _context.ReklamniPanoi.Add(reklamniPano);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReklamniPano), new { id = reklamniPano.Id }, reklamniPano);
        }

        [HttpPut("azuriraj/{id}")]
        public async Task<IActionResult> AzurirajReklamniPano(int id, ReklamniPano noviReklamniPano)
        {
            if (id != noviReklamniPano.Id)
            {
                return BadRequest("ID reklamnog panoa se ne podudara s ID-om u objektu.");
            }

            var postojeciReklamniPano = await _context.ReklamniPanoi.FindAsync(id);

            if (postojeciReklamniPano == null)
            {
                return NotFound("Reklamni pano s zadanim ID-om nije pronađen.");
            }

            postojeciReklamniPano.AdminAgencijeId = noviReklamniPano.AdminAgencijeId;
            postojeciReklamniPano.UrlSlike = noviReklamniPano.UrlSlike;
            postojeciReklamniPano.Adresa = noviReklamniPano.Adresa;
            postojeciReklamniPano.Dimenzija = noviReklamniPano.Dimenzija;
            postojeciReklamniPano.Osvetljenost = noviReklamniPano.Osvetljenost;
            postojeciReklamniPano.Grad = noviReklamniPano.Grad;
            postojeciReklamniPano.Zona = noviReklamniPano.Zona;
            postojeciReklamniPano.Cijena = noviReklamniPano.Cijena;


            // Spremite promjene u bazu podataka
            await _context.SaveChangesAsync();
            return NoContent(); // Vraća se status 204 - uspješno ažurirano
        }

        private bool ReklamniPanoExists(int id)
        {
            return _context.ReklamniPanoi.Any(e => e.Id == id);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReklamniPano(int id)
        {
            var reklamniPano = await _context.ReklamniPanoi.FindAsync(id);
            if (reklamniPano == null)
            {
                return NotFound();
            }

            _context.ReklamniPanoi.Remove(reklamniPano);
            await _context.SaveChangesAsync();

            return Ok("Uspesno izbrisano.");
        }


        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail(string email, string subject, string message)
        {
            try
            {
                using (var client = new SmtpClient("smtp.office365.com", 587))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("belkisa.dazdarevic1@gmail.com", "kvazilend5");

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("belkisa.dazdarevic1@gmail.com"),
                        Subject = subject,
                        Body = message,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(email);

                    await client.SendMailAsync(mailMessage);
                }

                return Ok("Email successfully sent.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to send email: {ex.Message}");
            }
        }
    } }


