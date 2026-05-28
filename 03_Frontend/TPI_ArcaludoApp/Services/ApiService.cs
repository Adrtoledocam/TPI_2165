using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TPI_ArcaludoApp.Models;
using TPI_ArcaludoApp.Services;
using static System.Net.WebRequestMethods;

namespace TPI_ArcaludoApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string _baseUrl = "http://localhost:8080/api/"; // émulateur Android

        public class LoginResponse
        {
            public string Token { get; set; }
            public User User { get; set; }
        }

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        //Authentification
        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            object loginData = new { email, password };
            StringContent content = Serialize(loginData);
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}auth/login", content);
                if (response.IsSuccessStatusCode)              
                {
                    string result = await response.Content.ReadAsStringAsync();
                    JObject json = JObject.Parse(result);

                    LoginResponse loginResponse = new LoginResponse();
                    loginResponse.Token = json["token"]?.ToString();
                    loginResponse.User = new User();
                    loginResponse.User.UseId = json["user"]?["id"]?.Value<int>() ?? 0;
                    loginResponse.User.UseUsername = json["user"]?["username"]?.ToString();
                    loginResponse.User.UseEmail = json["user"]?["email"]?.ToString();

                    return loginResponse;
                }
                return null;
            }
            catch (Exception ex) { Console.WriteLine($"[Login] {ex.Message}"); return null; }
        }

        public async Task<bool> RegisterAsync(string username, string email, string password)
        {
            object data = new { username, email, password };
            StringContent content = Serialize(data);
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}auth/register", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) { Console.WriteLine($"[Register] {ex.Message}"); return false; }
        }

        //User
        public async Task<User> GetProfileAsync(string token)
        {
            SetAuthHeader(token);
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_baseUrl}user/profile");
                if (!response.IsSuccessStatusCode) return null;
                string json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<User>(json);
            }
            catch (Exception ex) { Console.WriteLine($"[GetProfile] {ex.Message}"); return null; }
        }

        public async Task<bool> DeleteAccountAsync(string token)
        {
            SetAuthHeader(token);
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync($"{_baseUrl}user");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) { Console.WriteLine($"[DeleteAccount] {ex.Message}"); return false; }
        }

        //User Preferences
        public async Task<bool> UpdateCommunityOptInAsync(string token, bool optIn)
        {
            SetAuthHeader(token);
            StringContent content = Serialize(new { communityOptIn = optIn });
            try
            {
                HttpResponseMessage response = await _httpClient.PutAsync($"{_baseUrl}preferences", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) { Console.WriteLine($"[Update] {ex.Message}"); return false; }
        }

        //Jeux API RAWG
        public async Task<List<Game>> GetTrendingAsync(string token, string sort = "notes")
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Get, $"{_baseUrl}games/trending?sort={sort}");
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) return new List<Game>();
                string json = await response.Content.ReadAsStringAsync();
                List<Game> games = JsonConvert.DeserializeObject<List<Game>>(json);
                return games ?? new List<Game>();
            }
            catch (Exception ex) { Console.WriteLine($"[GetTrending] {ex.Message}"); return new List<Game>(); }
        }

        public async Task<bool> GetCommunityOptInAsync(string token)
        {
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(
                    HttpMethod.Get, $"{_baseUrl}preferences");
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) return false;
                string json = await response.Content.ReadAsStringAsync();
                JObject obj = JObject.Parse(json);
                return obj["communityOptIn"]?.Value<bool>() ?? false;
            }
            catch (Exception ex) { Console.WriteLine($"[GetCommunityOptIn] {ex.Message}"); return false; }
        }

        public async Task<List<Game>> SearchGamesAsync(string token, string query, string sort = "notes")
        {
            SetAuthHeader(token);
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_baseUrl}games/search?q={Uri.EscapeDataString(query)}&sort={sort}");
                if (!response.IsSuccessStatusCode) return new List<Game>();
                string json = await response.Content.ReadAsStringAsync();
                JObject parsed = JObject.Parse(json);
                List<Game> games = parsed["results"]?.ToObject<List<Game>>();
                if (games == null)
                {
                    return new List<Game>();
                }
                return games;
            }
            catch (Exception ex) { Console.WriteLine($"[SearchGames] {ex.Message}"); return new List<Game>(); }
        }

        public async Task<Game> GetGameDetailAsync(string token, int gameId)
        {
            SetAuthHeader(token);
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_baseUrl}games/{gameId}");
                if (!response.IsSuccessStatusCode) return null;
                string json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Game>(json);
            }
            catch (Exception ex) { Console.WriteLine($"[GetGameDetail] {ex.Message}"); return null; }
        }

        //Collection
        public async Task<List<CollectionGame>> GetCollectionAsync(string token, string status = "", string sort = "recent", string search = "")
        {
            SetAuthHeader(token);
            string url = $"{_baseUrl}collection?sort={sort}";
            if (!string.IsNullOrEmpty(status)) url += $"&status={status}";
            if (!string.IsNullOrEmpty(search)) url += $"&q={Uri.EscapeDataString(search)}";
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return new List<CollectionGame>();
                string json = await response.Content.ReadAsStringAsync();
                List<CollectionGame> collection = JsonConvert.DeserializeObject<List<CollectionGame>>(json);

                if (collection == null)
                {
                    return new List<CollectionGame>();
                }

                return collection;
            }
            catch (Exception ex) { Console.WriteLine($"[GetCollection] {ex.Message}"); return new List<CollectionGame>(); }
        }

        public async Task<bool> AddToCollectionAsync(string token, Game game, string status, string ownPlatforms = "")
        {
            SetAuthHeader(token);
            string platforms = null;
            if (!string.IsNullOrEmpty(ownPlatforms)) platforms = ownPlatforms;

            object data = new
            {
                game = new
                {
                    game.Id,
                    title = game.Title,
                    coverUrl = game.CoverUrl,
                    platforms = game.Platforms,
                    developer = game.Developer,
                    publisher = game.Publisher,
                    genre = game.Genre,
                    metacritic = game.Metacritic,
                    releaseDate = game.ReleaseDate
                },
                status,
                ownPlatforms = platforms
            };
            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync($"{_baseUrl}collection", Serialize(data));
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) { Console.WriteLine($"[AddToCollection] {ex.Message}"); return false; }
        }

        public async Task<bool> UpdateCollectionEntryAsync(string token, int colId, string status = null, int? rating = null, string comment = null, int? playtime = null, string ownPlatforms = null)
        {
            SetAuthHeader(token);
            Dictionary<string, object> data = new Dictionary<string, object>();
            if (status != null) data["status"] = status;
            if (rating != null) data["rating"] = rating;
            if (comment != null) data["comment"] = comment;
            if (playtime != null) data["playtime"] = playtime;
            if (ownPlatforms != null) data["ownPlatforms"] = ownPlatforms;
            try
            {
                HttpResponseMessage response = await _httpClient.PutAsync(
                    $"{_baseUrl}collection/{colId}", Serialize(data));
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) { Console.WriteLine($"[UpdateEntry] {ex.Message}"); return false; }
        }

        public async Task<bool> DeleteFromCollectionAsync(string token, int colId)
        {
            SetAuthHeader(token);
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync($"{_baseUrl}collection/{colId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) { Console.WriteLine($"[DeleteEntry] {ex.Message}"); return false; }
        }

        //Wishlist
        public async Task<List<CollectionGame>> GetWishlistAsync(string token, string sort = "recent", string search = "")
        {
            SetAuthHeader(token);
            string url = $"{_baseUrl}wishlist?sort={sort}";
            if (!string.IsNullOrEmpty(search)) url += $"&q={Uri.EscapeDataString(search)}";
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return new List<CollectionGame>();
                string json = await response.Content.ReadAsStringAsync();
                List<CollectionGame> wishlist = JsonConvert.DeserializeObject<List<CollectionGame>>(json);
                if (wishlist == null)
                {
                    return new List<CollectionGame>();
                }
                return wishlist;
            }
            catch (Exception ex) { Console.WriteLine($"[GetWishlist] {ex.Message}"); return new List<CollectionGame>(); }
        }

        public async Task<bool> MoveToCollectionAsync(string token, int colId, string status)
        {
            SetAuthHeader(token);
            StringContent content = Serialize(new { status });
            try
            {
                HttpResponseMessage response = await _httpClient.PutAsync($"{_baseUrl}wishlist/{colId}/move", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) { Console.WriteLine($"[MoveToCollection] {ex.Message}"); return false; }
        }

        public async Task<bool> DeleteFromWishlistAsync(string token, int colId)
        {
            SetAuthHeader(token);
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync($"{_baseUrl}wishlist/{colId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex) { Console.WriteLine($"[DeleteWishlist] {ex.Message}"); return false; }
        }

        //Community
        public async Task<List<CommunityMember>> GetCommunityAsync(string token)
        {
            SetAuthHeader(token);
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"{_baseUrl}community");
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden) return null;
                if (!response.IsSuccessStatusCode) return new List<CommunityMember>();
                string json = await response.Content.ReadAsStringAsync();
                List<CommunityMember> members = JsonConvert.DeserializeObject<List<CommunityMember>>(json);
                if (members == null)
                {
                    return new List<CommunityMember>();
                }
                return members;
            }
            catch (Exception ex) { Console.WriteLine($"[GetCommunity] {ex.Message}"); return new List<CommunityMember>(); }
        }


        //Methodes
        private void SetAuthHeader(string token) =>
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

        private static StringContent Serialize(object obj) =>
            new StringContent(
                JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");



    }
}
