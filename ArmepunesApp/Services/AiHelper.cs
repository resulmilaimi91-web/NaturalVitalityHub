using System.Text;
using System.Text.Json;

namespace ArmepunesApp.Services;

public class AiHelper
{
    private readonly AiSettings _settings;
    private readonly HttpClient _http;

    public AiHelper() : this(AiSettings.Load()) { }

    public AiHelper(AiSettings settings)
    {
        _settings = settings;
        _http = new HttpClient();
        _http.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
    }

    public async Task<string> GjeneroFleteleshim(string konteksti)
    {
        var prompt = @"Je nje asistent zyrtar i nje qendre deponimi armesh ne Shqiperi. 
Krijo nje tekst profesional ne shqip per fleteleshimin e deponimit te armes.

TE DHENAT E TRANSAKSIONIT:
" + konteksti + @"

Kthe vetem keto 2 pjese te ndara me vijen '---SHENIMET---':

PJESA 1 - QELLIMI: Nje ose dy fjali qe pershkruajne qellimin e deponimit/terheqjes (max 100 karaktere).
PJESA 2 - SHENIMET: Nje pershkrim i shkurte profesional (max 200 karaktere).

Shembull:
Deponim i armes per ruajtje te perkohshme ne depon e armepunes.
---SHENIMET---
Arma u deponua ne gjendje te mire. Aksesoret u verifikuan nga personeli pranues.";

        try
        {
            var requestBody = new
            {
                model = _settings.Model,
                stream = false,
                messages = new[]
                {
                    new { role = "user", content = $"{prompt}\n\n{konteksti}" }
                }
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync($"{_settings.Endpoint.TrimEnd('/')}/api/chat", content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);
            var msg = doc.RootElement.GetProperty("message").GetProperty("content").GetString() ?? "";

            return msg;
        }
        catch (Exception ex)
        {
            return $"<Gabim AI: {ex.Message}>";
        }
    }
}
