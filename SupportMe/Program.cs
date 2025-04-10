
using SupportMe.Data;
using SupportMe.Helpers;
using FirebaseAdmin.Auth;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Amazon.Util;
using SupportMe.DTOs.FileUploadDTOs;
using Amazon.S3;
using SupportMe.Services;
using Amazon;
using SupportMe.MiddleWares;
using NLog.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using SupportMe.Services.Auth;
using SixLabors.ImageSharp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Auhtorization",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"

    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type= ReferenceType.SecurityScheme,
                    Id="Bearer",
                }
            },
            new string[] {}
        }
    });

});

builder.Services.AddLogging(x => x.AddNLog());

builder.Services.AddAutoMapper(typeof(SupportMe.Helpers.AutoMapper.AutoMapper));

var connectionString = builder.Configuration.GetConnectionString("DB_CONNECTION");
builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer(connectionString));

builder.Services.AddSingleton(provider =>
{
    var byteArray = Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("Firebase:Json"));
    using var stream = new MemoryStream(byteArray);
    var app = FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.FromStream(stream)
    });
    return new FirebaseHandler(FirebaseAuth.GetAuth(app), FirebaseMessaging.GetMessaging(app));
});


builder.Services.AddSingleton(new S3BucketConfig(
                    builder.Configuration.GetValue<string>("Service:BaseUrl"),
                    builder.Configuration.GetValue<string>("GENERAL_BUCKET"),
                    builder.Configuration.GetValue<string>("CDN_URL")
                    ));

var s3Config = new S3Config
{
    Access = builder.Configuration.GetValue<string>("AmazonS3_Access"),
    Secret = builder.Configuration.GetValue<string>("AmazonS3_Secret")
};

var s3Client = new AmazonS3Client(s3Config.Access, s3Config.Secret, RegionEndpoint.USEast1);
builder.Services.AddScoped<FileUploadService>(_ => new FileUploadService(s3Client));


builder.Services
                .AddAuthentication()
                .AddJwtBearer("Token", jwt =>
                {
                    var pubKey =
                        builder.Configuration.GetValue<string>("Auth__Public");
                    var rsa = RSA.Create();
                    var pubKeyByteArray = Convert.FromBase64String(pubKey);
                    rsa.ImportRSAPublicKey(pubKeyByteArray, out _);
                    jwt.SaveToken = true;
                    jwt.RequireHttpsMetadata = false;
                    jwt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidAudience = builder.Configuration.GetValue<string>("Auth__Audience"),
                        ValidIssuer = builder.Configuration.GetValue<string>("Auth__Issuer"),
                        ClockSkew = TimeSpan.Zero,
                        IssuerSigningKey = new RsaSecurityKey(rsa),
                        CryptoProviderFactory = new CryptoProviderFactory
                        {
                            CacheSignatureProviders = false
                        }
                    };
                });



/* ################################################### */
/* ################    SERVICES     ################## */
/* ################################################### */

builder.Services.AddScoped<FirebaseAuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CampaignService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<MercadoPagoService>();


builder.Services.AddSingleton(
    new JwtConfig(
        builder.Configuration.GetValue<string>("Auth__Private"),
        builder.Configuration.GetValue<string>("Auth__Audience"),
        builder.Configuration.GetValue<string>("Auth__Issuer")));
/* ################################################### */
/* ################################################### */
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();
app.UseMiddleware<UserAuthMiddleware>();
app.UseMiddleware<ErrorHandlerMiddleware>();

app.MapControllers();

app.Run();
