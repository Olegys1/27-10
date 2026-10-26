using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapGet("/", ([FromServices] IUserRepository repository) =>
{
    var users = repository.GetAll();
    
    var html = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <title>User Management</title>
        <link rel='stylesheet' href='https://jsdelivr.net'>
    </head>
    <body class='container mt-5'>
        <div class='d-flex justify-content-between align-items-center mb-4'>
            <h2>User Management System</h2>
            <a href='/add' class='btn btn-primary'>Add New User</a>
        </div>
        <table class='table table-bordered table-striped'>
            <thead>
                <tr>
                    <th>ID</th>
                    <th>Name</th>
                    <th>Email</th>
                    <th>Actions</th>
                </tr>
            </thead>
            <tbody>";

    foreach (var user in users)
    {
        html += $@"
        <tr>
            <td>{user.Id}</td>
            <td>{user.Name}</td>
            <td>{user.Email}</td>
            <td>
                <a href='/edit?id={user.Id}' class='btn btn-sm btn-warning me-2'>Edit</a>
                <a href='/delete?id={user.Id}' class='btn btn-sm btn-danger'>Delete</a>
            </td>
        </tr>";
    }

    html += @"
            </tbody>
        </table>
    </body>
    </html>";

    return Results.Content(html, "text/html");
});

app.MapGet("/add", () =>
{
    var html = @"
    <!DOCTYPE html>
    <html>
    <head>
        <title>Add User</title>
        <link rel='stylesheet' href='https://jsdelivr.net'>
    </head>
    <body class='container mt-5'>
        <div class='row justify-content-center'>
            <div class='col-md-6'>
                <h2>Add New User</h2>
                <form action='/add' method='POST' class='mt-3'>
                    <div class='mb-3'>
                        <label class='form-label'>Name</label>
                        <input type='text' name='name' class='form-control' required />
                    </div>
                    <div class='mb-3'>
                        <label class='form-label'>Email</label>
                        <input type='email' name='email' class='form-control' required />
                    </div>
                    <button type='submit' class='btn btn-success'>Save</button>
                    <a href='/' class='btn btn-secondary'>Cancel</a>
                </form>
            </div>
        </div>
    </body>
    </html>";

    return Results.Content(html, "text/html");
});

app.MapPost("/add", ([FromForm] string name, [FromForm] string email, [FromServices] IUserRepository repository) =>
{
    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
    {
        return Results.BadRequest("Invalid input data.");
    }

    var newUser = new User { Name = name, Email = email };
    repository.Add(newUser);
    return Results.Redirect("/");
});

app.MapGet("/edit", (string id, [FromServices] IUserRepository repository) =>
{
    if (!Guid.TryParse(id, out Guid userId))
    {
        return Results.Content("<!DOCTYPE html><html><head><link rel='stylesheet' href='https://jsdelivr.net'></head><body class='container mt-5'><div class='alert alert-danger'>Invalid ID format.</div><a href='/' class='btn btn-primary'>Back to List</a></body></html>", "text/html");
    }

    var user = repository.GetById(userId);
    if (user == null)
    {
        return Results.Content("<!DOCTYPE html><html><head><link rel='stylesheet' href='https://jsdelivr.net'></head><body class='container mt-5'><div class='alert alert-danger'>User not found.</div><a href='/' class='btn btn-primary'>Back to List</a></body></html>", "text/html");
    }

    var html = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <title>Edit User</title>
        <link rel='stylesheet' href='https://jsdelivr.net'>
    </head>
    <body class='container mt-5'>
        <div class='row justify-content-center'>
            <div class='col-md-6'>
                <h2>Edit User</h2>
                <form action='/edit' method='POST' class='mt-3'>
                    <input type='hidden' name='id' value='{user.Id}' />
                    <div class='mb-3'>
                        <label class='form-label'>Name</label>
                        <input type='text' name='name' value='{user.Name}' class='form-control' required />
                    </div>
                    <div class='mb-3'>
                        <label class='form-label'>Email</label>
                        <input type='email' name='email' value='{user.Email}' class='form-control' required />
                    </div>
                    <button type='submit' class='btn btn-warning'>Update</button>
                    <a href='/' class='btn btn-secondary'>Cancel</a>
                </form>
            </div>
        </div>
    </body>
    </html>";

    return Results.Content(html, "text/html");
});

app.MapPost("/edit", ([FromForm] string id, [FromForm] string name, [FromForm] string email, [FromServices] IUserRepository repository) =>
{
    if (!Guid.TryParse(id, out Guid userId))
    {
        return Results.BadRequest("Invalid ID format.");
    }

    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
    {
        return Results.BadRequest("Invalid input data.");
    }

    var existingUser = repository.GetById(userId);
    if (existingUser == null)
    {
        return Results.NotFound("User not found.");
    }

    existingUser.Name = name;
    existingUser.Email = email;
    repository.Update(existingUser);

    return Results.Redirect("/");
});

app.MapGet("/delete", (string id, [FromServices] IUserRepository repository) =>
{
    if (!Guid.TryParse(id, out Guid userId))
    {
        return Results.Content("<!DOCTYPE html><html><head><link rel='stylesheet' href='https://jsdelivr.net'></head><body class='container mt-5'><div class='alert alert-danger'>Invalid ID format.</div><a href='/' class='btn btn-primary'>Back to List</a></body></html>", "text/html");
    }

    var user = repository.GetById(userId);
    if (user == null)
    {
        return Results.Content("<!DOCTYPE html><html><head><link rel='stylesheet' href='https://jsdelivr.net'></head><body class='container mt-5'><div class='alert alert-danger'>User not found.</div><a href='/' class='btn btn-primary'>Back to List</a></body></html>", "text/html");
    }

    repository.Delete(userId);
    return Results.Redirect("/");
});

app.Run();

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public interface IUserRepository
{
    IEnumerable<User> GetAll();
    User? GetById(Guid id);
    void Add(User user);
    void Update(User user);
    void Delete(Guid id);
}

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new()
    {
        new User { Name = "John Doe", Email = "john@example.com" },
        new User { Name = "Jane Smith", Email = "jane@example.com" }
    };

    public IEnumerable<User> GetAll() => _users;

    public User? GetById(Guid id) => _users.FirstOrDefault(u => u.Id == id);

    public void Add(User user) => _users.Add(user);

    public void Update(User user)
    {
        var existing = GetById(user.Id);
        if (existing != null)
        {
            existing.Name = user.Name;
            existing.Email = user.Email;
        }
    }

    public void Delete(Guid id)
    {
        var user = GetById(id);
        if (user != null)
        {
            _users.Remove(user);
        }
    }
}
