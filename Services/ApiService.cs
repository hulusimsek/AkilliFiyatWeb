using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AkilliFiyatWeb.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<string?> IndirimMigrosApiAsync(int pageNumber)
        {
            // API endpoint'i
            var apiUrl = $"https://www.migros.com.tr/rest/search/screens/money-indirimli-market-urunleri-dt-{pageNumber}";

            // HttpClient oluştur
            using (HttpClient client = new HttpClient())
            {
                // User-Agent bilgisini belirle
                client.DefaultRequestHeaders.UserAgent.ParseAdd("MyCustomUserAgent/1.0");

                // API'den veri al
                var response = await client.GetAsync(apiUrl);

                // Yanıtın içeriğini string olarak al
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    // Yanıt başarısızsa hata kodunu yazdır
                    Console.WriteLine($"HTTP isteği başarısız oldu: {response.StatusCode}");
                    // Yanıt başarısızsa null dön
                    return null;
                }
            }
        }

        public async Task<string?> MigrosApiAsync(string searchTerm, string reid)
        {
            // API endpoint'i
            var apiUrl = $"https://www.migros.com.tr/rest/search/screens/products?q={searchTerm}&reid={reid}";

            // HttpClient oluştur
            using (HttpClient client = new HttpClient())
            {
                // User-Agent bilgisini belirle
                client.DefaultRequestHeaders.UserAgent.ParseAdd("MyCustomUserAgent/1.0");

                // API'den veri al
                var response = await client.GetAsync(apiUrl);

                // Yanıtın içeriğini string olarak al
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    // Yanıt başarısızsa hata kodunu yazdır
                    Console.WriteLine($"HTTP isteği başarısız oldu: {response.StatusCode}");
                    // Yanıt başarısızsa null dön
                    return null;
                }
            }
        }

		public async Task<string?> A101ApiAsync(string searchTerm, string reid, string pn)
		{
			// API endpoint'i
			var apiUrl = $"https://a101.wawlabs.com/search?q={searchTerm}&rpp={reid}&pn={pn}";


			// HttpClient oluştur
			using (HttpClient client = new HttpClient())
			{
				// User-Agent bilgisini belirle
				client.DefaultRequestHeaders.UserAgent.ParseAdd("MyCustomUserAgent/1.0");

				// API'den veri al
				var response = await client.GetAsync(apiUrl);

				// Yanıtın içeriğini string olarak al
				if (response.IsSuccessStatusCode)
				{
					return await response.Content.ReadAsStringAsync();
				}
				else
				{
					// Yanıt başarısızsa hata kodunu yazdır
					Console.WriteLine($"HTTP isteği başarısız oldu: {response.StatusCode}");
					// Yanıt başarısızsa null dön
					return null;
				}
			}
		}

	}
}