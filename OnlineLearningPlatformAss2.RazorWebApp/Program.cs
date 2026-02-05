using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatformAss2.Data.Database;
using OnlineLearningPlatformAss2.Service.Services;
using OnlineLearningPlatformAss2.Service.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using OnlineLearningPlatformAss2.RazorWebApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add Entity Framework with SQL Server or In-Memory Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("Memory", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddDbContext<OnlineLearningContext>(options =>
        options.UseInMemoryDatabase("OnlineLearningPlatformDb"));
}
else
{
    builder.Services.AddDbContext<OnlineLearningContext>(options =>
        options.UseSqlServer(connectionString, b => b.MigrationsAssembly("OnlineLearningPlatformAss2.Data")));
}

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login";
        options.LogoutPath = "/User/Logout";
        options.AccessDeniedPath = "/User/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

// Register Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ILearningPathService, LearningPathService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IDiscussionService, DiscussionService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<DatabaseSeedService>();

// Add SignalR
builder.Services.AddSignalR();
builder.Services.AddScoped<ICourseUpdateBroadcaster, OnlineLearningPlatformAss2.RazorWebApp.Services.SignalRCourseUpdateBroadcaster>();

var app = builder.Build();

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var seedService = scope.ServiceProvider.GetRequiredService<DatabaseSeedService>();
    await seedService.SeedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Map SignalR Hubs
app.MapHub<CourseHub>("/hubs/course");
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
