using System;
using System.Collections.Generic;
using HandleMyQueue.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HandleMyQueue.Services
{
    public interface IAuthenticationService
    {
        public void Logout(HttpRequest req, HttpResponse resp);
        public Task<bool> Login(LoginDto loginDto, HttpRequest req, HttpResponse resp);
        public Task<ActionResult<string>> RefreshToken(string refreshToken, HttpRequest req, HttpResponse resp);
    }
    public class AuthenticationService : IAuthenticationService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly byte REFRESH_TOKEN_LIFE = 30;
        private readonly byte ACCESS_TOKEN_LIFE = 5;

        public void Logout(HttpRequest req, HttpResponse resp)
        {
            string accessToken = null, refreshToken = null;
            if (req.Cookies["X-Access-Token"] != null)
            {
                accessToken = req.Cookies["X-Access-Token"];
                resp.Cookies.Delete("X-Access-Token");
            }

            if (req.Cookies["X-Refresh-Token"] != null)
            {
                resp.Cookies.Delete("X-Refresh-Token");
                refreshToken = req.Cookies["X-Refresh-Token"];
            }

            if (accessToken != null && refreshToken != null)
            {
                var values = new Dictionary<string, string>
                {
                    { "client_id", "hmq" },
                    { "refresh_token", refreshToken },
                };

                _httpClient.DefaultRequestHeaders.Authorization
                    = new AuthenticationHeaderValue("Bearer", accessToken);
                var content = new FormUrlEncodedContent(values);
                _httpClient.PostAsync("http://localhost:8080/auth/realms/master/protocol/openid-connect/logout", content);
            }
        }

        public async Task<bool> Login(LoginDto loginDto, HttpRequest req, HttpResponse resp)
        {
            var values = new Dictionary<string, string>
            {
                { "client_id", "hmq" },
                { "username", loginDto.Username },
                { "grant_type", "password" },
                { "password", loginDto.Password }
            };

            var content = new FormUrlEncodedContent(values);

            var response = await _httpClient.PostAsync("http://localhost:8080/auth/realms/master/protocol/openid-connect/token", content);

            var responseString = await response.Content.ReadAsStringAsync();
            var credentials = JsonSerializer.Deserialize<CredentialsDto>(responseString);
            if (credentials.AccessToken == null || credentials.RefreshToken == null)
                return false;

            resp.Cookies.Append("X-Access-Token", credentials.AccessToken,
                new CookieOptions()
                {
                    HttpOnly = true,
                    IsEssential = true,
                    Expires = DateTime.Now.AddMinutes(ACCESS_TOKEN_LIFE)
                });
            resp.Cookies.Append("X-Refresh-Token", credentials.RefreshToken, new CookieOptions()
            {
                HttpOnly = true,
                IsEssential = true,
                Expires = DateTime.Now.AddMinutes(REFRESH_TOKEN_LIFE)
            });
            return true;
        }

        public async Task<ActionResult<string>> RefreshToken(string refreshToken, HttpRequest req, HttpResponse resp)
        {
            var values = new Dictionary<string, string>
            {
                { "client_id", "hmq" },
                { "grant_type", "refresh_token" },
                { "refresh_token", refreshToken }
            };

            var content = new FormUrlEncodedContent(values);

            var response = await _httpClient.PostAsync("http://localhost:8080/auth/realms/master/protocol/openid-connect/token", content);

            var responseString = await response.Content.ReadAsStringAsync();

            var credentials = JsonSerializer.Deserialize<CredentialsDto>(responseString);

            if (credentials.AccessToken == null || credentials.RefreshToken == null)
                return new UnauthorizedResult();

            if (req.Cookies["X-Access-Token"] != null)
            {
                resp.Cookies.Delete("X-Access-Token");
            }

            if (req.Cookies["X-Refresh-Token"] != null)
            {
                resp.Cookies.Delete("X-Refresh-Token");
            }

            resp.Cookies.Append("X-Access-Token", credentials.AccessToken,
                new CookieOptions()
                {
                    HttpOnly = true,
                    IsEssential = true,
                    Expires = DateTime.Now.AddMinutes(ACCESS_TOKEN_LIFE)
                });

            resp.Cookies.Append("X-Refresh-Token", credentials.RefreshToken, 
                new CookieOptions()
                {
                    HttpOnly = true,
                    IsEssential = true,
                    Expires = DateTime.Now.AddMinutes(REFRESH_TOKEN_LIFE)
                });

            return credentials.AccessToken;


        }
    }
}