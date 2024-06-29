// the includeInteractiveCredentials constructor parameter can be used to enable interactive authentication
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Azure.Core;
using Azure.Identity;
using Microsoft.Azure.Cosmos;


// message to be shown in the page
var stringData="";
// Init default credential
var credential = new Azure.Identity.DefaultAzureCredential();
//Address for APIM
var baseAddress = "https://testtokencqm002.azurewebsites.net/";
var api = "api/Todo";
var contentType = new MediaTypeWithQualityHeaderValue("application/json");
//init token
var token = new AccessToken();
//init http client
HttpClient client = new HttpClient(); 
//client.BaseAddress = new Uri(baseAddress);
client.DefaultRequestHeaders.Accept.Add(contentType);
//set initial time to measure delay using jwt autentication
DateTime T = System.DateTime.UtcNow; 
try
{
    string[] scopes = new string[] { "User.Read", "User.ReadBasic.All"};
    TokenRequestContext tr=new Azure.Core.TokenRequestContext(["api://644e0700-85ae-4de0-83dd-a876d692e693/.default"],null, "{roles}");
 token = credential.GetToken(tr);
     stringData=stringData + "roles:"+tr.Claims+ "\r\nToken:" +token.Token.ToString()+"\r\n";
}
catch (System.Exception e)
{
    stringData="Error getting token: "+e.Message;
}

//set token
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);


//set new timer to measure only authentication 
DateTime T2 = System.DateTime.UtcNow; 

try
{
    var response = await client.GetAsync(baseAddress + api);
   // TimeSpan TT = System.DateTime.UtcNow - T;
   // TimeSpan TT2 = System.DateTime.UtcNow - T2;
    if (response.IsSuccessStatusCode)
        {
        stringData = stringData+ "Response from APIM:"+ await response.Content.ReadAsStringAsync();
        Console.WriteLine(stringData);
        }
        else{
        stringData=stringData+ "Error with code: "+response.StatusCode.ToString();
        }
string base64GuidAll = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

string base64GuidCall = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
}
catch (System.Exception e)
{
    
    stringData= stringData+"Error getting response, message: "+e.Message;
}

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => stringData+System.DateTime.UtcNow.ToString());

app.Run();
public record Record(
    string appservicename,
    string app,
    string id,
    string category,
    string name,
    double responseTime,
    DateTime date
);