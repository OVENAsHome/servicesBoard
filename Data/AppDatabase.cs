using SQLite;
using ServicesBoard.Models;

namespace ServicesBoard.Data;

public class AppDatabase
{
    private SQLiteAsyncConnection? _database;

    private async Task Init()
    {
        if (_database != null)
            return;

        _database = new SQLiteAsyncConnection(DbConstants.DatabasePath, DbConstants.Flags);

        await _database.CreateTableAsync<User>();
        await _database.CreateTableAsync<ServiceAd>();
        await _database.CreateTableAsync<Request>();
        await _database.CreateTableAsync<Review>();
    }

    // ---------- Пользователи ----------

    public async Task<User?> GetUserByIdAsync(int id)
    {
        await Init();
        return await _database!.Table<User>()
            .Where(u => u.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByEmailAndPasswordAsync(string email, string password)
    {
        await Init();
        return await _database!.Table<User>()
            .Where(u => u.Email == email && u.Password == password)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await Init();
        return await _database!.Table<User>()
            .Where(u => u.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveUserAsync(User user)
    {
        await Init();
        if (user.Id != 0)
            return await _database!.UpdateAsync(user);
        else
            return await _database!.InsertAsync(user);
    }

    // Получить всех исполнителей
    public async Task<List<User>> GetPerformersAsync()
    {
        await Init();
        return await _database!.Table<User>()
            .Where(u => u.Role == "worker")
            .OrderByDescending(u => u.Rating)
            .ThenByDescending(u => u.OrdersCount)
            .ToListAsync();
    }

    // Увеличить счётчик заказов по исполнителю
    public async Task IncrementPerformerOrdersCountAsync(int performerId)
    {
        await Init();

        var user = await _database!.Table<User>()
            .Where(u => u.Id == performerId)
            .FirstOrDefaultAsync();

        if (user != null)
        {
            user.OrdersCount++;
            await _database.UpdateAsync(user);

            // пересчитываем рейтинг исполнителя на основе конверсии
            await RecalculatePerformerConversionRatingAsync(performerId);
        }
    }


    // ---------- Объявления ----------

    public async Task<List<ServiceAd>> GetActiveAdsAsync()
    {
        await Init();
        return await _database!.Table<ServiceAd>()
            .Where(a => a.Status == "active")
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<ServiceAd?> GetAdByIdAsync(int id)
    {
        await Init();
        return await _database!.Table<ServiceAd>()
            .Where(a => a.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<ServiceAd>> GetFilteredAdsAsync(
        string? category,
        double? minPrice,
        double? maxPrice,
        string? area,
        double? minRating)
    {
        await Init();

        var query = _database!.Table<ServiceAd>()
            .Where(a => a.Status == "active");

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(a => a.Category == category);

        if (!string.IsNullOrWhiteSpace(area))
            query = query.Where(a => a.Area.Contains(area));

        if (minPrice.HasValue)
            query = query.Where(a => a.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(a => a.Price <= maxPrice.Value);

        var ads = await query.ToListAsync();

        if (minRating.HasValue)
        {
            var users = await _database.Table<User>().ToListAsync();
            var dict = users.ToDictionary(u => u.Id);

            ads = ads
                .Where(a =>
                    dict.TryGetValue(a.UserId, out var u) &&
                    u.Rating >= minRating.Value)
                .ToList();
        }

        return ads;
    }

    public async Task<List<ServiceAd>> GetAdsByUserAsync(int userId)
    {
        await Init();
        return await _database!.Table<ServiceAd>()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> SaveAdAsync(ServiceAd ad)
    {
        await Init();
        if (ad.Id != 0)
            return await _database!.UpdateAsync(ad);
        else
            return await _database!.InsertAsync(ad);
    }

    public async Task<int> DeleteAdAsync(ServiceAd ad)
    {
        await Init();
        return await _database!.DeleteAsync(ad);
    }

    // ---------- Запросы ----------

    public async Task<int> SaveRequestAsync(Request request)
    {
        await Init();
        return await _database!.InsertAsync(request);
    }

    public async Task<List<Request>> GetRequestsForPerformerAsync(int performerId)
    {
        await Init();
        return await _database!.Table<Request>()
            .Where(r => r.PerformerId == performerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Request>> GetRequestsForCustomerAsync(int customerId)
    {
        await Init();
        return await _database!.Table<Request>()
            .Where(r => r.CustomerId == customerId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    // ---------- Отзывы ----------

    public async Task<int> SaveReviewAsync(Review review)
    {
        await Init();

        var result = await _database!.InsertAsync(review);

        await RecalculatePerformerRatingAsync(review.PerformerId);

        return result;
    }

    private async Task RecalculatePerformerRatingAsync(int performerId)
    {
        var reviews = await _database!.Table<Review>()
            .Where(r => r.PerformerId == performerId)
            .ToListAsync();

        if (reviews.Count == 0)
            return;

        var avg = reviews.Average(r => r.Rating);

        var user = await _database!.Table<User>()
            .Where(u => u.Id == performerId)
            .FirstOrDefaultAsync();

        if (user != null)
        {
            user.Rating = avg;
            user.ReviewsCount = reviews.Count;
            await _database.UpdateAsync(user);
        }
    }

    public async Task IncrementAdViewsAsync(int adId)
    {
        await Init();

        var ad = await _database!.Table<ServiceAd>()
            .Where(a => a.Id == adId)
            .FirstOrDefaultAsync();

        if (ad != null)
        {
            ad.ViewsCount++;
            await _database.UpdateAsync(ad);

            // пересчитываем рейтинг исполнителя на основе конверсии
            await RecalculatePerformerConversionRatingAsync(ad.UserId);
        }
    }


    public async Task<int> GetTotalViewsForPerformerAsync(int performerId)
    {
        await Init();
        var ads = await _database!.Table<ServiceAd>()
            .Where(a => a.UserId == performerId)
            .ToListAsync();

        return ads.Sum(a => a.ViewsCount);
    }

    public async Task RecalculatePerformerConversionRatingAsync(int performerId)
    {
        await Init();

        var performer = await _database!.Table<User>()
            .Where(u => u.Id == performerId)
            .FirstOrDefaultAsync();

        if (performer == null)
            return;

        int orders = performer.OrdersCount;
        int views = await GetTotalViewsForPerformerAsync(performerId);

        // сглаживание (чтобы новые исполнители не имели 0 и не прыгали)
        const double alpha = 1;   // добавляем 1 заказ "виртуально"
        const double beta = 10;   // добавляем 10 просмотров "виртуально"

        double cr = (orders + alpha) / (views + beta); // 0..1 (примерно)
        double rating = Math.Min(5.0, 5.0 * cr);       // 0..5

        performer.Rating = rating;
        await _database.UpdateAsync(performer);
    }


}
