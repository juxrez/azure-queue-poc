using Azure.Storage.Queues;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
string int2ConString = ""; 
string dev2ConString = "";
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddQueueServiceClient(int2ConString)
    .WithName("int2")
    .ConfigureOptions(options => 
    {
        options.MessageEncoding = QueueMessageEncoding.None;
    });

    clientBuilder.AddQueueServiceClient(dev2ConString)
    .WithName("dev2")
    .ConfigureOptions(options =>
    {
        options.MessageEncoding = QueueMessageEncoding.None;
    });
});

//add JWT Auth
//builder.Services.AddAuthentication()

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();




app.MapControllers();

app.Run();
