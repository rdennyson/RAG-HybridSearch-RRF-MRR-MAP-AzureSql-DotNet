using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using MediatR;
using Scalar.AspNetCore;
using DocMan.Core.Services;
using DocMan.Core.Services.SparseRetrieval;
using DocMan.Core.Services.Fusion;
using DocMan.Core.Services.HyDE;
using DocMan.Core.Services.Reranking;
using DocMan.Core.Services.HybridSearch;
using DocMan.Core.Settings;
using DocMan.Core.ContentDecoders;
using DocMan.Core.TextChunkers;
using DocMan.Data;
using DocMan.Data.UnitOfWork;
using DocMan.API.Endpoints;
using DocMan.Core.Features.Auth.Command;

var builder = WebApplication.CreateBuilder(args);

// Load settings
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
var azureOpenAISettings = builder.Configuration.GetSection("AzureOpenAI").Get<AzureOpenAISettings>() ?? new AzureOpenAISettings();
var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();

// Add services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection"),
    x => x.UseVectorSearch()));

// Unit of Work & Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Authentication
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton(azureOpenAISettings);
builder.Services.Configure<AzureOpenAISettings>(builder.Configuration.GetSection("AzureOpenAI"));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Hybrid Cache for chat history
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new()
    {
        LocalCacheExpiration = TimeSpan.FromHours(1)
    };
});

// Semantic Kernel for embeddings and chat completion
#pragma warning disable SKEXP0010
builder.Services.AddKernel()
    .AddAzureOpenAIEmbeddingGenerator(
        azureOpenAISettings.Embedding.Deployment,
        azureOpenAISettings.Embedding.Endpoint,
        azureOpenAISettings.Embedding.ApiKey,
        modelId: azureOpenAISettings.Embedding.ModelId,
        dimensions: azureOpenAISettings.Embedding.Dimensions)
    .AddAzureOpenAIChatCompletion(
        azureOpenAISettings.ChatCompletion.Deployment,
        azureOpenAISettings.ChatCompletion.Endpoint,
        azureOpenAISettings.ChatCompletion.ApiKey,
        modelId: azureOpenAISettings.ChatCompletion.ModelId);
#pragma warning restore SKEXP0010

// Core Services
builder.Services.AddScoped<IEmbeddingService, EmbeddingService>();
builder.Services.AddScoped<IDocumentManagementService, DocumentManagementService>();
builder.Services.AddScoped<IVectorSearchService, VectorSearchService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IRagService, RagService>();
builder.Services.AddScoped<IEvaluationMetricsService, EvaluationMetricsService>();
builder.Services.AddSingleton<ITokenizerService, TokenizerService>();

// Hybrid Search Services
builder.Services.AddScoped<IBM25Service, BM25Service>();
builder.Services.AddScoped<IReciprocalRankFusionService, ReciprocalRankFusionService>();
builder.Services.AddScoped<IHyDEService, HyDEService>();
builder.Services.AddScoped<ICrossEncoderReranker, CrossEncoderReranker>();
builder.Services.AddScoped<IHybridSearchService, HybridSearchService>();

// Content Decoders
builder.Services.AddScoped<IContentDecoder, PdfContentDecoder>();
builder.Services.AddScoped<IContentDecoder, DocxContentDecoder>();
builder.Services.AddScoped<IContentDecoder, TextContentDecoder>();

// Text Chunker
builder.Services.AddScoped<ITextChunker, DefaultTextChunker>();
builder.Services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LoginCommand).Assembly));

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// OpenAPI & Scalar
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapAuthEndpoints();
app.MapDocumentEndpoints();
app.MapSearchEndpoints();
app.MapCategoryEndpoints();
app.MapUserEndpoints();

app.Run();
