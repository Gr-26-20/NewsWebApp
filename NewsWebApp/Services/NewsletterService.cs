using NewsWebApp.Models;
using NewsWebApp.Data;
using Microsoft.EntityFrameworkCore;

namespace NewsWebApp.Services
{
    public class NewsletterService : INewsletterService
    {
        private readonly ApplicationDbContext _context;

        public NewsletterService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<NewsLetter>> GetAllNewslettersAsync()
        {
            return await _context.NewsLetters.ToListAsync();
        }

        public async Task<NewsLetter> GetNewsletterByIdAsync(int id)
        {
            return await _context.NewsLetters.FindAsync(id);
        }

        public async Task AddNewsLetter(NewsLetter newsletter)
        {
            _context.NewsLetters.Add(newsletter);
            await _context.SaveChangesAsync();
        }

        public async Task EditNewsLetter(NewsLetter newsletter)
        {
            _context.NewsLetters.Update(newsletter);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNewsLetter(NewsLetter newsletter)
        {
            _context.NewsLetters.Remove(newsletter);
            await _context.SaveChangesAsync();
        }
    }
}
