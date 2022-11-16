#region MIT License

// ==========================================================
// 
// ResourceTranslator project - Copyright (c) 2022 JeePeeTee
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
// ===========================================================

#endregion


#region usings

using System.Net;
using System.Text;
using ConsoleApp6;
using Newtonsoft.Json;

#endregion

const string translationLanguage = "nl";

const string codeLocation = @"<Your resource location here...>";

const string source = codeLocation + "en.json";
const string destination = codeLocation + $"{translationLanguage}.json";

var transFrom = JsonConvert.DeserializeObject<Translations>(File.ReadAllText(source));
var transTo = JsonConvert.DeserializeObject<Translations>(File.ReadAllText(destination));

foreach (var enText in transFrom!
             .Texts
             .Where(enText => !transTo!.Texts.ContainsKey(enText.Key))) {
    transTo!.Texts.Add(enText.Key, "Translation here...");
    transTo.Texts[enText.Key] = await TranslateText(enText.Value, translationLanguage);
}

var translations = JsonConvert.SerializeObject(transTo);
File.WriteAllText(destination, translations);

static async Task<string> TranslateText(string textToTranslate, string toLang) {
    const string key = "<Your translator key here...>";
    const string endpoint = "https://api.cognitive.microsofttranslator.com";
    const string location = "<Your location here>";

    var route = $"/translate?api-version=3.0&from=en&to={toLang}";
    var body = new object[] { new { Text = textToTranslate } };
    var requestBody = JsonConvert.SerializeObject(body);

    using var client = new HttpClient();
    using var request = new HttpRequestMessage();
    // Build the request.
    request.Method = HttpMethod.Post;
    request.RequestUri = new Uri(endpoint + route);
    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
    request.Headers.Add("Ocp-Apim-Subscription-Key", key);
    // location required if you're using a multi-service or regional (not global) resource.
    request.Headers.Add("Ocp-Apim-Subscription-Region", location);

    // Send the request and get response.
    var response = await client.SendAsync(request).ConfigureAwait(false);
    // Read response as a string.
    if (response.StatusCode != HttpStatusCode.OK)
        throw new Exception($"API request result: {response.StatusCode}");

    var result = await response.Content.ReadAsStringAsync();

    var translations = JsonConvert.DeserializeObject<List<TranslationResult>>(result);
    var translation = translations![0].Translations[0].Text;
    return translation;
}