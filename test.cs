
// using System.Diagnostics;
// using System.Text;
// using System.Text.Json;
// using PrintingApi;
// using Xunit;

// // https://code-maze.com/dotnet-test-rest-api-xunit/
// public class Tests {

//     private readonly HttpClient _httpClient = new() { BaseAddress = new Uri("https://localhost:7133") };
//     private void Dispose() {
//         _httpClient.DeleteAsync("/state").GetAwaiter().GetResult();
//     }
//     private static class TestHelpers {
//         private const string _jsonMediaType = "application/json";
//         private const int _expectedMaxElapsedMilliseconds = 1000;
//         private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

//         public static async Task AssertResponseWithContentAsync<T>(Stopwatch stopwatch,
//             HttpResponseMessage response, System.Net.HttpStatusCode expectedStatusCode,
//             T expectedContent) {
//             AssertCommonResponseParts(stopwatch, response, expectedStatusCode);
//             Assert.Equal(_jsonMediaType, response.Content.Headers.ContentType?.MediaType);
//             Assert.Equal(expectedContent, await JsonSerializer.DeserializeAsync<T?>(
//                 await response.Content.ReadAsStreamAsync(), _jsonSerializerOptions));
//         }
//         private static void AssertCommonResponseParts(Stopwatch stopwatch,
//             HttpResponseMessage response, System.Net.HttpStatusCode expectedStatusCode) {
//             Assert.Equal(expectedStatusCode, response.StatusCode);
//             Assert.True(stopwatch.ElapsedMilliseconds < _expectedMaxElapsedMilliseconds);
//         }
//         public static StringContent GetJsonStringContent<T>(T model)
//             => new(JsonSerializer.Serialize(model), Encoding.UTF8, _jsonMediaType);

//     }

//     [Fact]
//     private async Task TestDataPostAsync() {
//         // Arrange.
//         var expectedStatusCode = System.Net.HttpStatusCode.OK;

//         var content = new Invoice {
//             PrinterName = "Microsoft XPS Document Writer",
//             TemplateName = "receipt",
//             Company = "مطعم ميدان الشام",
//             Cashier = "وليد",
//             Branch = "فرع الإستاد",
//             // Date_ = new DateOnly(),
//             // Time_ = new TimeOnly(),
//             InvoiceNo = 343,
//             SectionName = "قسم المخبوزات",
//             TableNo = 123,
//             InvoiceType = "صالة",
//             ClientName = "Hero",
//             ClientPhone1 = "2341236523",
//             ClientAddress = "عنوان عميل طويل عنوان عميل طويل عنوان عميل طويل عنوان عميل طويل عنوان عميل طويل عنوان عميل طويل عنوان عميل طويل عنوان عميل طويل عنوان عميل طويل عنوان عميل طويل ",
//             ClientArea = "area is also handled",
//             // Items = new List<Item>{
//             //         new Item{ Title="صنف 1", Price=32, Count=23, Note="ملاحظة صنف"},
//             // },
//             EditedItems = new List<Item>{
//                     new Item{ Title="صنف 1", Price=32, Count=23, Note="ملاحظة صنف"},
//                     new Item{ Title="صنف 1", Price=32, Count=23,},
//                     new Item{ Title="صنف 1", Price=32, Count=23,},
//                     new Item{ Title="صنف 1", Price=32, Count=23,},
//                     new Item{ Title="صنف 1", Price=32, Count=23,},
//                     new Item{ Title="صنف 1", Price=32, Count=23, Note="ملاحظة صنف"},
//                     new Item{ Title="صنف 1", Price=32, Count=23, Note="ملاحظة صنف"},
//             },
//             Service = 20,
//             Vat = 33,
//             Total = 344,
//         };

//         var stopwatch = Stopwatch.StartNew();
//         // Act.
//         var buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(content));
//         var byteContent = new ByteArrayContent(buffer);
//         byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
//         var response = await _httpClient.PostAsync("/PrintingData", byteContent);
//         // Assert.
//         await TestHelpers.AssertResponseWithContentAsync(stopwatch, response, expectedStatusCode, content);
//     }


//     private void TestData() {
//         var items = new Dictionary<string, Object> {
//             // new {name="test naem", prop=""}
//             // new Item{ Title="صنف 1", Price=32, Count=23, Note="ملاحظة صنف"},
//         };

//     }
// }
