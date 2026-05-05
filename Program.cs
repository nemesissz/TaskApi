using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskApi.Data;

namespace TaskApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(opt =>
                opt.UseSqlite("Data Source=taskapi.db"));
            // builder.Services.AddDbContext<AppDbContext>(opt =>
            //     opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
            


            builder.Services.AddIdentity<AppUser, IdentityRole<Guid>>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = ctx =>
                {
                    ctx.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = ctx =>
                {
                    ctx.Response.StatusCode = 403;
                    return Task.CompletedTask;
                };
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };
            });

            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.Services.AddScoped<ITaskService, TaskService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.SetIsOriginAllowed(_ => true)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                await db.Database.EnsureCreatedAsync();
                // MSSQL-ə keçiddə EnsureCreatedAsync() əvəzinə Migration istifadə et:
                //   1) Terminaldə: dotnet ef migrations add InitialCreate
                //   2) Terminaldə: dotnet ef database update
                // Bundan sonra EnsureCreatedAsync() və aşağıdakı bütün ExecuteSqlRawAsync bloklarını SİL —
                // MSSQL-də cədvəllər migration vasitəsilə yaranır, manual SQL lazım deyil.

                // SQLite üçün Notes cədvəli
                await db.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS Notes (
                        Id TEXT NOT NULL PRIMARY KEY,
                        UserLogin TEXT NOT NULL DEFAULT '',
                        Metn TEXT NOT NULL DEFAULT '',
                        Notlar TEXT NOT NULL DEFAULT '',
                        Tamamlanib INTEGER NOT NULL DEFAULT 0,
                        YaranmaTarixi TEXT NOT NULL DEFAULT (datetime('now')),
                        TarixAktiv INTEGER NOT NULL DEFAULT 0,
                        SaatAktiv INTEGER NOT NULL DEFAULT 0,
                        Tarix TEXT,
                        Saat TEXT
                    );");

                // SQLite üçün ChatMessages cədvəli
                await db.Database.ExecuteSqlRawAsync(@"
                    CREATE TABLE IF NOT EXISTS ChatMessages (
                        Id TEXT NOT NULL PRIMARY KEY,
                        SenderLogin TEXT NOT NULL DEFAULT '',
                        SenderName TEXT NOT NULL DEFAULT '',
                        ReceiverLogin TEXT NOT NULL DEFAULT '',
                        ReceiverName TEXT NOT NULL DEFAULT '',
                        Text TEXT NOT NULL DEFAULT '',
                        CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
                        IsRead INTEGER NOT NULL DEFAULT 0,
                        IsDeleted INTEGER NOT NULL DEFAULT 0,
                        IsEdited INTEGER NOT NULL DEFAULT 0,
                        FileName TEXT,
                        FileType TEXT,
                        FileBase64 TEXT
                    );");

                // SQLite üçün IsNezaretci sütunu
                try
                {
                    await db.Database.ExecuteSqlRawAsync(
                        "ALTER TABLE TaskAssignments ADD COLUMN IsNezaretci INTEGER NOT NULL DEFAULT 0;");
                }
                catch { }

                // SQLite üçün IsSeen sütunu
                try
                {
                    await db.Database.ExecuteSqlRawAsync(
                        "ALTER TABLE TaskAssignments ADD COLUMN IsSeen INTEGER NOT NULL DEFAULT 0;");
                }
                catch { }

                // SQLite üçün Muessise Logo sütunu
                try
                {
                    await db.Database.ExecuteSqlRawAsync(
                        "ALTER TABLE Muessiseler ADD COLUMN Logo TEXT;");
                }
                catch { }

                // MSSQL üçün Notes cədvəli
                // await db.Database.ExecuteSqlRawAsync(@"
                //     IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Notes' AND xtype='U')
                //     CREATE TABLE Notes (
                //         Id NVARCHAR(450) NOT NULL PRIMARY KEY,
                //         UserLogin NVARCHAR(MAX) NOT NULL DEFAULT '',
                //         Metn NVARCHAR(MAX) NOT NULL DEFAULT '',
                //         Notlar NVARCHAR(MAX) NOT NULL DEFAULT '',
                //         Tamamlanib BIT NOT NULL DEFAULT 0,
                //         YaranmaTarixi NVARCHAR(MAX) NOT NULL DEFAULT '',
                //         TarixAktiv BIT NOT NULL DEFAULT 0,
                //         SaatAktiv BIT NOT NULL DEFAULT 0,
                //         Tarix NVARCHAR(MAX),
                //         Saat NVARCHAR(MAX)
                //     );");

                // MSSQL üçün ChatMessages cədvəli
                // await db.Database.ExecuteSqlRawAsync(@"
                //     IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ChatMessages' AND xtype='U')
                //     CREATE TABLE ChatMessages (
                //         Id NVARCHAR(450) NOT NULL PRIMARY KEY,
                //         SenderLogin NVARCHAR(MAX) NOT NULL DEFAULT '',
                //         SenderName NVARCHAR(MAX) NOT NULL DEFAULT '',
                //         ReceiverLogin NVARCHAR(MAX) NOT NULL DEFAULT '',
                //         ReceiverName NVARCHAR(MAX) NOT NULL DEFAULT '',
                //         Text NVARCHAR(MAX) NOT NULL DEFAULT '',
                //         CreatedAt NVARCHAR(MAX) NOT NULL DEFAULT '',
                //         IsRead BIT NOT NULL DEFAULT 0,
                //         IsDeleted BIT NOT NULL DEFAULT 0,
                //         IsEdited BIT NOT NULL DEFAULT 0,
                //         FileName NVARCHAR(MAX),
                //         FileType NVARCHAR(MAX),
                //         FileBase64 NVARCHAR(MAX)
                //     );");

                // MSSQL üçün IsNezaretci sütunu
                // try
                // {
                //     await db.Database.ExecuteSqlRawAsync(@"
                //         IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TaskAssignments') AND name = 'IsNezaretci')
                //         ALTER TABLE TaskAssignments ADD IsNezaretci BIT NOT NULL DEFAULT 0;");
                // }
                // catch { /* Sütun artıq mövcuddursa keç */ }

                // SuperAdmin seed
                var superAdminExists = userManager.Users.Any(u => u.Role == "SuperAdmin");
                if (!superAdminExists)
                {
                    var superAdmin = new AppUser
                    {
                        UserName = "superadmin",
                        FullName = "Super Administrator",
                        Role = "SuperAdmin"
                    };
                    await userManager.CreateAsync(superAdmin, "SuperAdmin@123");
                }

            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
            {
                var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
                ctx.Response.StatusCode = ex switch
                {
                    KeyNotFoundException => 404,
                    UnauthorizedAccessException => 403,
                    _ => 500
                };
                ctx.Response.ContentType = "application/json";
                await ctx.Response.WriteAsJsonAsync(new { message = ex?.Message ?? "Xəta baş verdi" });
            }));

            if (!app.Environment.IsDevelopment())
                app.UseHttpsRedirection();

            app.UseCors("AllowFrontend");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
