using CouncilAgendaApi.Data;
using CouncilAgendaApi.Middleware;
using CouncilAgendaApi.Services;
using CouncilAgendaApi.Services.Interfaces;
using CouncilAgendaApi.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your Clerk JWT token here"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://clerk.{builder.Configuration["Clerk:Domain"]}";
        options.TokenValidationParameters = new()
        {
            ValidateAudience = false
        };
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthorizationHandler, OrgAccessHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OrgViewer", policy =>
        policy.Requirements.Add(new OrgAccessRequirement("viewer")));
    options.AddPolicy("OrgEditor", policy =>
        policy.Requirements.Add(new OrgAccessRequirement("editor")));
    options.AddPolicy("OrgAdmin", policy =>
        policy.Requirements.Add(new OrgAccessRequirement("admin")));
});
builder.Services.AddScoped<AgendaGeneratorService>();
builder.Services.AddScoped<DiscussionQuestionService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IMeetingService, MeetingService>();
builder.Services.AddScoped<IHandbookService, HandbookService>();
builder.Services.AddScoped<AgendaGeneratorService>();
builder.Services.AddScoped<DiscussionQuestionService>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddMvc();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseExceptionHandler();

app.Run();