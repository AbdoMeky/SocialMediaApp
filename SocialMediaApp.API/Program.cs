using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SocialMediaApp.Core.Entities;
using SocialMediaApp.Core.Helper;
using SocialMediaApp.Core.Interface;
using SocialMediaApp.Core.Interface.Factory;
using SocialMediaApp.Core.Services;
using SocialMediaApp.Infrastructure.Data;
using SocialMediaApp.Infrastructure.Factory;
using SocialMediaApp.Infrastructure.Hubs;
using SocialMediaApp.Infrastructure.Repository;
using SocialMediaApp.Infrastructure.Repository.MessageRepository;
using SocialMediaApp.Services;
using System.Text;
namespace SocialMediaApp.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", policy =>
                {
                    policy
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .SetIsOriginAllowed(_ => true)
                        .AllowCredentials();
                });
            });


            // Add controllers & Swagger
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(swagger =>
                        {
                            swagger.SwaggerDoc("v1", new OpenApiInfo
                            {
                                Version = "v1",
                                Title = "Social Media Web API",
                                Description = "social media"
                            });

                            // JWT in Swagger
                            swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                            {
                                Name = "Authorization",
                                Type = SecuritySchemeType.ApiKey,
                                Scheme = "Bearer",
                                BearerFormat = "JWT",
                                In = ParameterLocation.Header,
                                Description =
                                    "Enter 'Bearer' [space] and then your valid token.\r\n\r\nExample: \"Bearer eyJhbGciOi...\""
                            });
                            swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                                        new string[] { }
                                    }
                                });
                        });



            // EF Core & Identity
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("RemoteConnection"));
            });

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();





            // Application services & repositories
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ICurrentOnlineUserRepository, CurrentOnlineUserRepository>();
            builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);
            builder.Services.AddScoped<IFriendShipRepository, FriendShipRepository>();
            builder.Services.AddScoped<IFriendShipRequiestRepository, FriendShipRequiestRepository>();
            builder.Services.AddScoped<IGroupChatMemberRepository, GroupChatMemberRepository>();
            builder.Services.AddScoped<IGroupChatRepository, GroupChatRepository>();
            builder.Services.AddScoped<IMessageStatusForChatMemberRepository, MessageStatusForChatMemberRepository>();
            builder.Services.AddScoped<ITwosomeChatMemberRepository, TwosomeChatMemberRepository>();
            builder.Services.AddScoped<ITwosomeChatRepository, TwosomeChatRepository>();
            builder.Services.AddScoped<GroupChatMessageRepository>();
            builder.Services.AddScoped<TwoSomeChatMessageRepository>();
            builder.Services.AddScoped<IMessageRepositoryFactory, MessageRepositoryFactory>();
            builder.Services.AddScoped<IChatRepository, ChatRepository>();
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddScoped(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<EmailSettings>>().Value);
            builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("JWT"));
            builder.Services.AddScoped(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<JWTSettings>>().Value);
            builder.Services.Configure<GoogleSettings>(builder.Configuration.GetSection("Authentication:Google"));
            builder.Services.AddScoped(sp => sp.GetRequiredService<IOptions<GoogleSettings>>().Value);
            var jwtSettings = builder.Configuration.GetSection("JWT").Get<JWTSettings>()!;
            var googleSettings = builder.Configuration.GetSection("Authentication:Google").Get<GoogleSettings>()!;








            // Authentication & Google

            //logger
            /*builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Debug);*/



            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.IssuerIP,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.AudienceIP,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecritKey ?? string.Empty))
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/ChatHub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            })
            .AddGoogle(options =>
            {
                options.ClientId = googleSettings.ClientID;
                options.ClientSecret = googleSettings.ClientSecret;
            });

            builder.Services.AddSignalR();

            var app = builder.Build();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors("AllowAllOrigins");

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.MapHub<ChatHub>("/ChatHub");

            app.MapControllers();

            app.Run();
        }
    }
}
