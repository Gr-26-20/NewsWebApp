using NewsWebApp.Models;

namespace NewsWebApp.Services
{
    public interface INewsletterService
    {
        Task<List<NewsLetter>> GetAllNewslettersAsync();

        Task<NewsLetter> GetNewsletterByIdAsync(int id);

        Task AddNewsLetter(NewsLetter newsletter);

        Task EditNewsLetter(NewsLetter newsletter);

        Task DeleteNewsLetter(NewsLetter newsletter);
    }


}
