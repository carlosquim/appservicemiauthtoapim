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
String envApimGatewayEp = Environment.GetEnvironmentVariable("envApimGatewayEp");
var baseAddress =envApimGatewayEp;

String envApi = Environment.GetEnvironmentVariable("envApi");
var api = envApi;
var contentType = new MediaTypeWithQualityHeaderValue("application/json");
//init token
var token = new AccessToken();
//init http client
HttpClient client = new HttpClient(); 
client.BaseAddress = new Uri(baseAddress);
client.DefaultRequestHeaders.Accept.Add(contentType);
//set initial time to measure delay using jwt autentication
DateTime T = System.DateTime.UtcNow;
try
{
String envAppRegistrationId = Environment.GetEnvironmentVariable("envAppRegistrationId");
 token = credential.GetToken(new Azure.Core.TokenRequestContext([envAppRegistrationId]));
 Console.WriteLine(token.Token);
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
    TimeSpan TT = System.DateTime.UtcNow - T;
    TimeSpan TT2 = System.DateTime.UtcNow - T2;
    if (response.IsSuccessStatusCode)
        {
        stringData = stringData+ "Response from APIM:"+ await response.Content.ReadAsStringAsync();
        Console.WriteLine(stringData);
        }
        else{
        stringData=stringData+ "Error with code: "+response.StatusCode.ToString();
        }
string base64GuidAll = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
Record itemAll = new(
    appservicename:"clientwithauth",
    app:"authentication",
    id: base64GuidAll,
    category: "auth+call",
    name: "Time for authentication and API call in MS",
    responseTime: TT.TotalMilliseconds,
    date: System.DateTime.UtcNow


);
string base64GuidCall = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
Record itemRequest= new(
    appservicename:"clientwithauth",
    app:"authentication",
    id: base64GuidCall,
    category: "call",
    name: "Time for API Call in MS",
    responseTime: TT2.TotalMilliseconds,
    date: System.DateTime.UtcNow
);
string envCosmosPk= Environment.GetEnvironmentVariable("envCosmospk");
string envCosmosEp= Environment.GetEnvironmentVariable("envCosmosep");
string EndpointUri= envCosmosEp;
string PrimaryKey = envCosmosPk;

CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "CosmosDBDotnetQuickstart" });
Database database = cosmosClient.GetDatabase("metrics");
Container container = database.GetContainer("metrics");




ItemResponse<Record> firstResponse = await container.CreateItemAsync<Record>(itemAll);

// Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
 Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", firstResponse.Resource.id, firstResponse.RequestCharge);



ItemResponse<Record> callResponse = await container.CreateItemAsync<Record>(itemRequest);

// Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
 Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", callResponse.Resource.id, callResponse.RequestCharge);
               

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