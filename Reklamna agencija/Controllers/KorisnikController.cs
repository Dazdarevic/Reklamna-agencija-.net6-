using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reklamna_agencija.Data;
using Reklamna_agencija.Models;
using System.Security.Cryptography;
using System.Text;

namespace Reklamna_agencija.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KorisnikController : ControllerBase
    {
        private readonly DataContext _context;

        public KorisnikController(DataContext context)
        {
            _context = context;
        }

        [HttpPost("dodaj-korisnika")]
        public async Task<ActionResult<Korisnik>> AddKorisnik(Korisnik korisnik)
        {
            
            // Dodajemo korisnika u zavisnosti od uloge
            switch (korisnik.Uloga)
            {
                case "Admin":
                    _context.AdminiAgencije.Add(
                        new AdminAgencije 
                        {
                            Id = korisnik.Id,
                            Status = korisnik.Status,
                            Prezime = korisnik.Prezime,
                            Ime = korisnik.Ime,
                            Email = korisnik.Email,
                            Password = EncryptPassword(korisnik.Password),
                            BrTel = korisnik.BrTel,
                            Uloga = korisnik.Uloga,
                            Pol = korisnik.Pol,
                            Profilna = korisnik.Profilna,
                            DatumRodjenja = korisnik.DatumRodjenja
                        });
                    break;
                case "Klijent":
                    _context.Klijenti.Add(
                        new Klijent 
                        {
                            Id = korisnik.Id,
                            Status = korisnik.Status,
                            Prezime = korisnik.Prezime,
                            Ime = korisnik.Ime,
                            Email = korisnik.Email,
                            Password = EncryptPassword(korisnik.Password),
                            BrTel = korisnik.BrTel,
                            Uloga = korisnik.Uloga,
                            Pol = korisnik.Pol,
                            Profilna = korisnik.Profilna,
                            DatumRodjenja = korisnik.DatumRodjenja
                        });
                    break;
                case "Posetilac":
                    _context.Posetioci.Add(
                        new Posetilac 
                        {
                            Id = korisnik.Id,
                            Status = korisnik.Status,
                            Prezime = korisnik.Prezime,
                            Ime = korisnik.Ime,
                            Email = korisnik.Email,
                            Password = EncryptPassword(korisnik.Password),
                            BrTel = korisnik.BrTel,
                            Uloga = korisnik.Uloga,
                            Pol = korisnik.Pol,
                            Profilna = korisnik.Profilna,
                            DatumRodjenja = korisnik.DatumRodjenja
                        });
                    break;
                default:
                    return BadRequest("Nepoznata uloga korisnika.");
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetKorisnik), new { id = korisnik.Id }, korisnik);
        }

        public static string EncryptPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Pretvori lozinku u bajtovni niz
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Konvertuj bajtove u string u heksadecimalan format
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetKorisnik(int id)
        {
            var admin = await _context.AdminiAgencije.FindAsync(id);
            var klijent = await _context.Klijenti.FindAsync(id);
            var posetilac = await _context.Posetioci.FindAsync(id);

            if (admin != null)
            {
                return admin;
            }
            else if (klijent != null)
            {
                return klijent;
            }
            else if (posetilac != null)
            {
                return posetilac;
            }
            else
            {
                return NotFound();
            }
        }
        [HttpGet("mojprofil")]
        public async Task<ActionResult<object>> GetKorisnikInfo(int id, string role)
        {
            // Provera da li je uloga validna
            if (role != "Admin" && role != "Klijent" && role != "Posetilac")
            {
                return BadRequest("Neispravna uloga korisnika.");
            }

            // Pronalaženje korisnika na osnovu id-ja i uloge
            if (role == "Admin")
            {
                var admin = await _context.AdminiAgencije.FindAsync(id);
                if (admin != null)
                {
                    return admin;
                }
            }
            else if (role == "Klijent")
            {
                var klijent = await _context.Klijenti.FindAsync(id);
                if (klijent != null)
                {
                    return klijent;
                }
            }
            else if (role == "Posetilac")
            {
                var posetilac = await _context.Posetioci.FindAsync(id);
                if (posetilac != null)
                {
                    return posetilac;
                }
            }

            return NotFound("Korisnik nije pronađen.");
        }

        //svi korisnici
        [HttpGet("korisnici")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllKorisnici()
        {
            var korisnici = await _context.AdminiAgencije
                .Select(a => new { Id = a.Id, Ime = a.Ime, Prezime = a.Prezime, Uloga = "Admin" })
                .Union(
                    _context.Klijenti.Select(k => new { Id = k.Id, Ime = k.Ime, Prezime = k.Prezime, Uloga = "Klijent" })
                )
                .Union(
                    _context.Posetioci.Select(p => new { Id = p.Id, Ime = p.Ime, Prezime = p.Prezime, Uloga = "Posetilac" })
                )
                .ToListAsync();

            return Ok(korisnici);
        }

        [HttpGet("neodobreni-korisnici")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllInactiveKorisnici()
        {
            var korisnici = await _context.AdminiAgencije
                .Where(a => a.Status == false)
                .Select(a => new
                {
                    Id = a.Id,
                    Ime = a.Ime,
                    Prezime = a.Prezime,
                    Uloga = "Admin",
                    Email = a.Email,
                    BrojTelefona = a.BrTel,
                    Pol = a.Pol,
                    Profilna = a.Profilna
                })
                .Union(
                    _context.Klijenti
                        .Where(k => k.Status == false)
                        .Select(k => new
                        {
                            Id = k.Id,
                            Ime = k.Ime,
                            Prezime = k.Prezime,
                            Uloga = "Klijent",
                            Email = k.Email,
                            BrojTelefona = k.BrTel,
                            Pol = k.Pol,
                            Profilna = k.Profilna
                        })
                )
                .Union(
                    _context.Posetioci
                        .Where(p => p.Status == false)
                        .Select(p => new
                        {
                            Id = p.Id,
                            Ime = p.Ime,
                            Prezime = p.Prezime,
                            Uloga = "Posetilac",
                            Email = p.Email,
                            BrojTelefona = p.BrTel,
                            Pol = p.Pol,
                            Profilna = p.Profilna
                        })
                )
                .ToListAsync();

            return Ok(korisnici);
        }


        [HttpPut("odobrena-registracija")]
        public async Task<IActionResult> UpdateKorisnikStatus(int id, string email)
        {
            var admin = await _context.AdminiAgencije.FirstOrDefaultAsync(a => a.Id == id && a.Email == email);
            if (admin != null)
            {
                admin.Status = true;
                _context.Update(admin);
                await _context.SaveChangesAsync();
                return Ok("Status administratore uspešno ažuriran.");
            }

            var klijent = await _context.Klijenti.FirstOrDefaultAsync(k => k.Id == id && k.Email == email);
            if (klijent != null)
            {
                klijent.Status = true;
                _context.Update(klijent);
                await _context.SaveChangesAsync();
                return Ok("Status klijenta uspešno ažuriran.");
            }

            var posetilac = await _context.Posetioci.FirstOrDefaultAsync(p => p.Id == id && p.Email == email);
            if (posetilac != null)
            {
                posetilac.Status = true;
                _context.Update(posetilac);
                await _context.SaveChangesAsync();
                return Ok("Status posetioca uspešno ažuriran.");
            }

            return NotFound("Korisnik nije pronađen.");
        }


        //brisanje korisnika
        [HttpDelete]
        public async Task<IActionResult> DeleteKorisnik(int id, string email)
        {
            var admin = await _context.AdminiAgencije.FindAsync(id);
            if (admin != null && admin.Email == email)
            {
                _context.AdminiAgencije.Remove(admin);
                await _context.SaveChangesAsync();
                return Ok("Admin successfully deleted.");
            }

            var klijent = await _context.Klijenti.FindAsync(id);
            if (klijent != null && klijent.Email == email)
            {
                _context.Klijenti.Remove(klijent);
                await _context.SaveChangesAsync();
                return Ok("Klijent successfully deleted.");
            }

            var posetilac = await _context.Posetioci.FindAsync(id);
            if (posetilac != null && posetilac.Email == email)
            {
                _context.Posetioci.Remove(posetilac);
                await _context.SaveChangesAsync();
                return Ok("Posetilac successfully deleted.");
            }

            return NotFound("Korisnik not found or email does not match.");
        }


        //azuriranje profilne slike
        [HttpPost("update-profile-picture")]
        public async Task UpdateProfilePictureUrlAsync(int userId, string role, string profilePictureUrl)
        {
            Korisnik user = null;

            switch (role.ToLower())
            {
                case "admin":
                    user = await _context.AdminiAgencije.FirstOrDefaultAsync(u => u.Id == userId);
                    break;
                case "receptionist":
                    user = await _context.Klijenti.FirstOrDefaultAsync(u => u.Id == userId);
                    break;
                case "trainer":
                    user = await _context.Posetioci.FirstOrDefaultAsync(u => u.Id == userId);
                    break;
                default:
                    throw new ArgumentException("Nema takvog korisnika");
            }

            if (user != null)
            {
                user.Profilna = profilePictureUrl;
                await _context.SaveChangesAsync();
            }
        }

        //da li korisnik vec postoji u bazi
        [HttpGet("email-exists/{email}")]
        public async Task<bool> EmailExists(string email)
        {
            var adminExists = await _context.AdminiAgencije.AnyAsync(a => a.Email == email);
            if (adminExists)
                return true;

            var klijentExists = await _context.Klijenti.AnyAsync(k => k.Email == email);
            if (klijentExists)
                return true;

            var posetilacExists = await _context.Posetioci.AnyAsync(p => p.Email == email);
            if (posetilacExists)
                return true;

            return false;
        }
    }
}
