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
            // MSSQL-ə keçid üçün yuxarıdakı 2 sətri silib aşağıdakı ilə əvəz et:
            // builder.Services.AddDbContext<AppDbContext>(opt =>
            //     opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
            // Əlavə olaraq csproj-da paketi dəyiş:
            //   Sil:   <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" ... />
            //   Əlavə: <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />


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
                // MSSQL-də bu bloku SİL — migration Notes cədvəlini özü yaradır.

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
                // MSSQL-də bu bloku SİL — migration ChatMessages cədvəlini özü yaradır.

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

                // Default Admin seed
                var existingAdmin = await userManager.FindByNameAsync("admin");
                if (existingAdmin is null)
                {
                    var admin = new AppUser
                    {
                        UserName = "admin",
                        FullName = "Administrator",
                        Role = "SuperAdmin"
                    };
                    await userManager.CreateAsync(admin, "Admin@123");
                }
                else if (existingAdmin.Role != "SuperAdmin")
                {
                    existingAdmin.Role = "SuperAdmin";
                    await userManager.UpdateAsync(existingAdmin);
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
